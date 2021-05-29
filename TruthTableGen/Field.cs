using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TruthTableGen
{
	//контейнер для зберігання даних кожного стовпця
	public class Field
	{
		public string leftOpd, rightOpd;
		public List<bool> fieldResult = new List<bool>();
		public char fieldOpr;
	}
}
