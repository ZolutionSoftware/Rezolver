// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Rezolver
{
  /// <summary>
  /// Reorders an expression tree where duplicate conditional expressions are found
  /// in multiple places throughout that expression tree. Those duplicated conditionals
  /// are moved further up the expression tree into one conditional.
  /// </summary>
  public class ConditionalRewriter : ExpressionVisitor
  {
    private enum RewriteStages
    {
      NotRun,
      GatheringConditionals,
      RewritingConditionals
    }

    private enum ConditionalRewritePart
    {
      TruePart,
      FalsePart
    }

    private class ConditionalExpressionInfo : IEquatable<ConditionalExpressionInfo>
    {
      public ConditionalExpression Expression;
      public Stack<Expression> Hierarchy;

      //equality is exceptionally simple - only performs reference equality check.
      //this works only because the specific scenario that this rewriter is designed to
      //handle relates to the RezolvedTarget - which uses shared expressions.
      public bool Equals(ConditionalExpressionInfo other)
      {
        return object.ReferenceEquals(Expression.Test, other.Expression.Test);
      }

      public override bool Equals(object obj)
      {
        return Equals(obj as ConditionalExpressionInfo);
      }

      public override int GetHashCode()
      {
        return Expression.Test.GetHashCode();
      }
    }

    private class ConditionalExpressionGroupInfo
    {
      public IGrouping<ConditionalExpressionInfo, ConditionalExpressionInfo> Group;
      public Expression CommonParent;
    }

    private class ConditionalExpressionRewriteState
    {
      public ConditionalExpressionGroupInfo GroupInfo;
      public ConditionalRewritePart Mode;
    }

    int _rewriteCount = 0;
    private Expression _expression;
    private IEnumerable<Expression> _candidateTests;
    private Stack<Expression> _currentStack = new Stack<Expression>();
    private List<ConditionalExpressionInfo> _allConditionals = new List<ConditionalExpressionInfo>();
    private ConditionalExpressionGroupInfo[] _groupedConditionals = null;
    private Stack<ConditionalExpressionRewriteState> _currentlyRewriting = new Stack<ConditionalExpressionRewriteState>();
    private RewriteStages _currentStage = RewriteStages.NotRun;
    public ConditionalRewriter(Expression expression, IEnumerable<Expression> candidateTests)
    {
      _expression = expression;
      _candidateTests = candidateTests;
    }

    public Expression Rewrite()
    {
      _currentStage = RewriteStages.GatheringConditionals;
      var result = Visit(_expression);
      //if there are no conditionals, or only one, then there's no benefit to rewriting.
      if (_allConditionals.Count <= 1)
        return result;
      var grouped = _allConditionals.GroupBy(i => i).ToArray();

      if (grouped.Length == _allConditionals.Count)
        return result;  //no rewriting to do here, all conditional tests are unique

      //now we have to find whether this group has a shared parent expression that we can rewrite
      //this involves finding the bottom-most expression which is present in all the stacks that
      //we gathered.  We won't remove that expression, we're just going to push it down and clone it 
      //as a new pair of iffalse and iftrue branches in a new conditional expression.
      _groupedConditionals = (from grp in grouped
                              let grpArray = grp.ToArray()
                              where grpArray.Length > 1
                              let commonParent = (from expr in grpArray[0].Hierarchy
                                                  let otherHierarchies = (from conditional2 in grp.Skip(1)
                                                                          select conditional2.Hierarchy)
                                                  where otherHierarchies.All(h => h.Any(expr2 => object.ReferenceEquals(expr, expr2)))
                                                  select expr).LastOrDefault()
                              where commonParent != null
                              select new ConditionalExpressionGroupInfo { Group = grp, CommonParent = commonParent }).ToArray();

      _currentStage = RewriteStages.RewritingConditionals;

      result = Visit(result);

      return result;
    }
    public override Expression Visit(Expression node)
    {
      if (_currentStage == RewriteStages.GatheringConditionals)
      {
        _currentStack.Push(node);
        var result = base.Visit(node);
        _currentStack.Pop();
        return result;
      }
      else if (_currentStage == RewriteStages.RewritingConditionals)
      {
        var matchingGroup = _groupedConditionals.FirstOrDefault(gc => object.ReferenceEquals(node, gc.CommonParent));
        if (matchingGroup != null)
        {
          var state = new ConditionalExpressionRewriteState() { GroupInfo = matchingGroup, Mode = ConditionalRewritePart.TruePart };
          //so now we are going to place this expression inside a
          _currentlyRewriting.Push(state);
          var truePart = base.Visit(node);
          _currentlyRewriting.Pop();
          state = new ConditionalExpressionRewriteState() { GroupInfo = matchingGroup, Mode = ConditionalRewritePart.FalsePart };
          _currentlyRewriting.Push(state);
          var falsePart = base.Visit(node);
          _currentlyRewriting.Pop();

          return Expression.Condition(matchingGroup.Group.Key.Expression.Test, truePart, falsePart);
        }
      }

      return base.Visit(node);
    }

    protected override Expression VisitConditional(ConditionalExpression node)
    {
      if (_currentStage == RewriteStages.GatheringConditionals)
      {
        //check that this expression is one of those that 
        if (_candidateTests.Any(e => object.ReferenceEquals(node.Test, e)))
        {
          //remember that the stack here will have this node as the most recently added object
          _allConditionals.Add(new ConditionalExpressionInfo() { Expression = node, Hierarchy = new Stack<Expression>(_currentStack) });
        }
        return base.VisitConditional(node);
      }
      else if (_currentStage == RewriteStages.RewritingConditionals && _currentlyRewriting.Count != 0)
      {
        var currentRewrite = _currentlyRewriting.Peek();
        var matching = currentRewrite.GroupInfo.Group.FirstOrDefault(e => object.ReferenceEquals(e.Expression, node));
        if (matching != null)
        {
          ++_rewriteCount;
          switch (currentRewrite.Mode)
          {
            case ConditionalRewritePart.TruePart:
              {
                return Visit(matching.Expression.IfTrue);
              }
            case ConditionalRewritePart.FalsePart:
              {
                return Visit(matching.Expression.IfFalse);
              }
          }
        }
      }

      return base.VisitConditional(node);
    }
  }

}
