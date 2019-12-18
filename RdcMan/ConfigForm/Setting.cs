namespace RdcMan
{
	public abstract class Setting<T> : BaseSetting<T>
	{
		public new T Value
		{
			get { return base.Value; }
			set { base.Value = value; }
		}

		protected Setting(object o)
			: base(o)
		{
		}
	}
}
