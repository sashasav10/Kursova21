using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TruthTableGen
{
	class EvaluatorText:Evaluator
	{
		public EvaluatorText(string Query) : base(Query)
		{
			this.Query = "(" + Query + ")";
		}

	}
}
