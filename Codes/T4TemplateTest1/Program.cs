using T4TemplateTest1;

TestModel model = new TestModel("Person2", typeof(Person).GetProperties());
MyTextTemplate tt = new MyTextTemplate();
tt.Model = model;
string code = tt.TransformText();
Console.WriteLine(code);