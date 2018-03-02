// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using Rezolver.Tests.Examples.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Examples
{
    public class MixedVarianceExamples
    {
        [Fact]
        public void ShouldMatchMixedVariant()
        {
            // <example1>
            var container = new Container();
            var expected = new Converter<object, Dictionary<string, string>>(
                o => new Dictionary<string, string>
                {
                    ["type"] = o.GetType().ToString(),
                    ["value"] = o.ToString()
                });
            container.RegisterObject(expected);

            var result = container.Resolve<Converter<Rectangle, IDictionary<string, string>>>();
            Assert.Same(expected, result);
            // </example1>
        }

        [Fact]
        public void ShouldMatchEnumerableOfMixedVariant()
        {
            // <example2>
            var container = new Container();

            var objectConverter = new Converter<object, Dictionary<string, string>>(
                o => new Dictionary<string, string>
                {
                    ["type"] = o.GetType().ToString(),
                    ["value"] = o.ToString()
                });
            var shapeConverter = new Converter<I2DShape, Dictionary<string, string>>(
                o => new Dictionary<string, string>
                {
                    ["area"] = o.CalcArea().ToString()
                });
            var rectangleConverter = new Converter<Rectangle, Dictionary<string, string>>(
                o => new Dictionary<string, string>
                {
                    ["width"] = o.Length.ToString(),
                    ["height"] = o.Height.ToString()
                });
            var circleConverter = new Converter<Circle, Dictionary<string, string>>(
                o => new Dictionary<string, string>
                {
                    ["radius"] = o.Radius.ToString()
                });

            container.RegisterObject(objectConverter);
            container.RegisterObject(shapeConverter);
            container.RegisterObject(rectangleConverter);
            container.RegisterObject(circleConverter);

            // for <Circle, IDictionary<string, string>>, we should get three:
            var result1 = container.ResolveMany<Converter<Circle, IDictionary<string, string>>>();
            Assert.Equal(new Converter<Circle, IDictionary<string, string>>[] {
                objectConverter,
                shapeConverter,
                circleConverter
            }, result1);

            // and for <Square, IDictionary<string, string>>, we should also get three:
            // this time, there are *no* exact matches
            var result2 = container.ResolveMany<Converter<Square, IDictionary<string, string>>>();
            Assert.Equal(new Converter<Square, IDictionary<string, string>>[] {
                objectConverter,
                shapeConverter,
                rectangleConverter
            }, result2);
            // </example2>
        }
    }
}