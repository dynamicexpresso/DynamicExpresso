using NUnit.Framework;

namespace DynamicExpresso.UnitTest {
   public class BoxedValue {
      private BoxedValue(object value)
      {
         Value = value;
      }

      public object Value { get; }
      public static implicit operator BoxedValue(string other) => new BoxedValue(other);
      public static implicit operator BoxedValue(int other) => new BoxedValue(other);
   }

   public class Container {
      // ReSharper disable once UnusedAutoPropertyAccessor.Global
      public BoxedValue BoxedValue { get; set; }
   }

   [TestFixture]
   public class AssignmentTest {
      [Test]
      public void Can_Assign_Using_ImplicitCast()
      {
         var container = new Container();

         var interpreter = new Interpreter();
         interpreter.SetVariable("Container", container);
         
         interpreter.Eval("Container.BoxedValue = \"Test\"");
         Assert.AreEqual("Test", container.BoxedValue.Value);
         
         interpreter.Eval("Container.BoxedValue = 2");
         Assert.AreEqual(2, container.BoxedValue.Value);
      }
   }
}