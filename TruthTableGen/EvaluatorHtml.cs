using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace TruthTableGen
{
	class EvaluatorHtml:Evaluator
	{
		public EvaluatorHtml(string Query):base (Query)
		{
			base.Query = "(" + Query + ")";
		}
		public override string AddVarInversion(string varName)
		{
			return "<span style=\"text-decoration: overline; \">" + varName + "</span>";
		}
		public override string AddVar(string varName)
		{
			return "<span>" + varName + "</span>";
		}
		public override dynamic GenerateTable()
		{
			StringBuilder outputHtml= new StringBuilder ("<div style='width:700px; margin:auto;'><table class='table table-bordered' style='width:auto' align=bottom left>");
			outputHtml.Append("<thead><tr>");
			
			foreach (var column in EvalPlan)
			{
				outputHtml.Append("<th align='center'>"+ column.Key.ToString().Replace("¬", "") + " </th>");
			}
			outputHtml.Append("</tr></thead>");


			// перебираємо дані та додаємо у таблицю
			for (int i = 0; i < EvalPlan.ElementAt(0).Value.fieldResult.Count; i++)
			{
				for (int j = 0; j < EvalPlan.Count; j++)
				{
					if (EvalPlan.ElementAt(j).Value.fieldResult[i]) outputHtml.Append("<td align='center'>" + '1' + "</td>");
					else outputHtml.Append("<td align='center'>" + '0' + "</td>");

				}
				outputHtml.Append("</tr>");

			}
			outputHtml.Append("</table></div>");
			return outputHtml;
		}

	}

}
