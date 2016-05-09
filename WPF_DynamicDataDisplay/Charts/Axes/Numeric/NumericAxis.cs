using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Microsoft.Research.DynamicDataDisplay.Charts
{
	public class NumericAxis : AxisBase<double>
	{
		public NumericAxis()
			: base(new NumericAxisControl(),
				d => d,
				d => d)
		{
		}
	}

    public class Ms10Axis : AxisBase<double>
    {
        public Ms10Axis()
            : base(new NumericAxisControl(),
                d => d/1000,
                d => d * 1000)
        {
        }
    }

}
