namespace AutoMapperTest
{
    using System;
    using System.Text.Json;

    class Program
    {
        static void Main(string[] args)
        {
            TestClass1 instance1 = new TestClass1() { 
                Property1 = 1,
                Property2 = 2,
                ArrayProperty = new int[] { 1, 2, 3 }
            };
            TestClass2 dest = new Clz1ToClz2Mapper().Map(instance1);
            Console.WriteLine(JsonSerializer.Serialize(dest));
        }

        
        static TDest MapTest<TSource, TDest>(TSource source)
            where TDest : class, new()
            where TSource : class
        {
            TDest dest = new TDest();
            Type sourceType = typeof(TSource), destType = typeof(TDest);
            
            foreach (var destPropertyInfo in destType.GetProperties())
            {
                var sourcePropertyInfo = sourceType.GetProperty(destPropertyInfo.Name);

                if (sourcePropertyInfo != null)
                {
                    if (destPropertyInfo.PropertyType.IsValueType)
                    {
                        destPropertyInfo.SetValue(dest, sourcePropertyInfo.GetValue(source));
                    }
                    else if (destPropertyInfo.PropertyType.IsArray)
                    {
                        destPropertyInfo.SetValue(dest, ((Array)sourcePropertyInfo.GetValue(source)).Clone());
                    }
                    else
                    {
                        //if (sourcePropertyInfo.PropertyType.GetCustomAttributes(true).Any(x => ((Attribute)x)..AttributeType==typeof(AutoMapperLibrary.Attributes.AutomapToAttribute) && x.DestType == destPropertyInfo.PropertyType)
                        //destPropertyInfo.SetValue(dest, MapTest(sourcePropertyInfo.GetValue(source)));

                    }
                }
                    
            }
            return dest;
        }
    }
}
