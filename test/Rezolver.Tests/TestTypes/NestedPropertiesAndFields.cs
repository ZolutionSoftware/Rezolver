using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#pragma warning disable 649 //unassigned field

namespace Rezolver.Tests.TestTypes
{
  public class NestedPropertiesAndFields
  {
    public HasProperty Field_HasProperty;
    public HasField Property_HasField { get; set; }
  }
}
