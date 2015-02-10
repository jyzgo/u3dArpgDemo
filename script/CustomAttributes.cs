

namespace InJoy
{
	public class IntMinMaxAttribute : System.Attribute
	{
		public IntMinMaxAttribute(int min, int max)
		{
			_min = min;
			_max = max;
		}
		
		private int _min = 0;
		public int Min
		{
			get
			{
				return _min;
			}
		}
		
		private int _max = 100;
		public int Max
		{
			get
			{
				return _max;
			}
		}
	}
}
