using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace TruthTableGen
{
	class EvaluatorLatex : Evaluator
	{
		public EvaluatorLatex(string Query) : base(Query)
		{
			base.Query = "(" + Query + ")";
		}
		public override string AddVarInversion(string varName)
		{
			return "$\\overline{" + varName +"}$";
		}
		public override dynamic AddSymbol(char symbol)
		{
			if (symbol == '∧') return "$\\land$";
			else return "$\\vee $";
		}
		public override string FormSubstring(string normalF, int length)
		{
			return normalF.Substring(0, normalF.Length - length-6);
		}
		public override dynamic GenerateTable()
		{
			StringBuilder outputLa = new StringBuilder("\\begin{tabular}{ |");
			foreach (var column in EvalPlan)
			{
				outputLa.Append(" l |");
			}
			outputLa.Append("}" +Environment.NewLine+"\\hline " + Environment.NewLine);
			foreach (var column in EvalPlan)
			{
				outputLa.Append(column.Key.ToString().Replace("¬", "").Replace("∧", "$\\land$").Replace("∨", "$\\vee$").Replace("→", "$\\to$").Replace("↔","$\\leftrightarrow$") + " & ");
			}
			outputLa.Remove(outputLa.Length-3, 2);
			outputLa.Append(@" \\ \hline" + Environment.NewLine);


			// перебираємо дані та додаємо у таблицю
			for (int i = 0; i < EvalPlan.ElementAt(0).Value.fieldResult.Count; i++)
			{
				for (int j = 0; j < EvalPlan.Count; j++)
				{
					if (EvalPlan.ElementAt(j).Value.fieldResult[i]) outputLa.Append('1' + " & ");
					else outputLa.Append('0' + " & ");

				}
				outputLa.Remove(outputLa.Length - 3,2);
				outputLa.Append(@" \\" + Environment.NewLine);

			}
			outputLa.Append("\\hline" + Environment.NewLine +"\\end{tabular}");
			return outputLa;
		}

	}
}
