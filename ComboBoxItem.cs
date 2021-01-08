using System;

namespace KeyMute
{
   public sealed class ComboBoxItem : IEquatable<ComboBoxItem>
   {
      public string Text { get; }
      public object Value { get; }

      public ComboBoxItem(object value, string text)
      {
         this.Value = value;
         this.Text = text;
      }

      public bool Equals(ComboBoxItem other)
      {
         if (other == null)
            return false;
         return Equals(Value, other.Value);
      }

      public override bool Equals(object obj)
      {
         return Equals(obj as ComboBoxItem);
      }

      public override int GetHashCode()
      {
         if (Value == null)
            return 0;
         return Value.GetHashCode();
      }    
     
      public override string ToString()
      {
         return Text;
      }


   }
}
