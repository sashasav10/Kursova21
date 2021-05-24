using System;
using System.Collections.Generic;
using System.Data;
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


    // Обчислення запиту (виразу)
    public abstract class Evaluator
    {
		public Dictionary<string, Field> EvalPlan { get; set; }
		public string Query { get; set; }

		public static char[] prec = { '(', '¬', '∧', '∨', '→', '↔', ')' };

        int FindPrec(char inp)
        {
            for (int i = 0; i < prec.Length; i++) { if (prec[i] == inp) { return i; } }
            return -1;
        }

		//Конструктор, який ініціалізує запит
		public Evaluator(string Query) { this.Query = "(" + Query + ")"; }


		//Ця функція використовується для утворення словника
		private Dictionary<string, Field> FindEvalPlan()
        {
			//Необхідний для отримання плану без необхідності перетворення на постфікс
			Stack<char> boolOpr = new Stack<char>();
            Stack<string> boolOpd = new Stack<string>();

			//Зберігає план оцінки як пару <key, value> для зручного доступу
			EvalPlan = new Dictionary<string, Field>();

			//Перевірка кожного елемента в запиті
			foreach (var i in Query)
            {
                if (i != ' ')
                {
					//Перевірка між оператором та операндом
					if (prec.Contains<char>(i) == false)
                    {
						// Операнди вставляються в чергу операндів
						boolOpd.Push(i.ToString());

						Field columnField = new Field();
                        if (boolOpd.Count != 0 && EvalPlan.Keys.Contains(boolOpd.Peek()) == false) { EvalPlan.Add(boolOpd.Peek(), columnField); }
                    }
                    else
                    {
						//Якщо символ є дійсним символом у правильній позиції
						if (i != '(' && (boolOpr.Peek() != '(' || FindPrec(boolOpr.Peek()) <= FindPrec(i)))
                        {
							//коли перевага стає меншою, тоді виконується формування
							while (boolOpr.Peek() != '(' && FindPrec(boolOpr.Peek()) <= FindPrec(i))
                            {
								//Отримати відповідно до оператора
								Field columnField = new Field();
                                if (boolOpr.Peek() == '¬')
                                {
                                    columnField.rightOpd = boolOpd.Pop();
                                    columnField.fieldOpr = boolOpr.Pop();
                                    boolOpd.Push(AddVarInversion("(" + " " + columnField.fieldOpr + " " + columnField.rightOpd + " " + ")"));
                                }
                                else
                                {
                                    columnField.rightOpd = boolOpd.Pop();
                                    columnField.fieldOpr = boolOpr.Pop();
                                    columnField.leftOpd = boolOpd.Pop();
                                    boolOpd.Push("(" + " " + columnField.leftOpd + " " + columnField.fieldOpr + " " + columnField.rightOpd + " " + ")");
                                }

								// Якщо план ще не був сформований, додаєм його
								// В іншому випадку відкидаємо, щоб уникнути надмірності
								if (boolOpd.Count != 0 && EvalPlan.Keys.Contains(boolOpd.Peek()) == false) { EvalPlan.Add(boolOpd.Peek(), columnField); }
                            }

							//Якщо символ ")", видаліть відповідний "(", щоб збалансувати рівняння
							if (i == ')') { boolOpr.Pop(); } else { boolOpr.Push(i); }
                        }
                        else { boolOpr.Push(i); }   // Оператори з вищим пріоритетом просто засовуються в стек операторів
					}
                }
            }

            return EvalPlan;
        }

		//Це використовується для отримання стовпця з кінцевим результатом
		private bool[] GetResultData()
        {
			//Поле результату - це стовпець з найбільшим значенням ключа
			string result = "";
            bool[] resultData = null;
            foreach (var i in EvalPlan) { if (result.Length < i.Key.Length) { result = i.Key; resultData = i.Value.fieldResult.ToArray(); } }
            return resultData;
        }

		//Використовується для повернення результату оцінки логічної операції з масивом операндів
		private List<bool> ExecOp(List<bool> leftOpd, List<bool> rightOpd, char boolOpr)
        {
            List<bool> resultOpd = new List<bool>();

			//Оцініть роботу оператора
			switch (boolOpr)
            {
                case '¬':
                    for (int i = 0; i < rightOpd.Count; i++) { resultOpd.Add(!rightOpd[i]); }
                    break;
                case '∧':
                    for (int i = 0; i < leftOpd.Count; i++) { resultOpd.Add(leftOpd[i] && rightOpd[i]); }
                    break;
                case '∨':
                    for (int i = 0; i < leftOpd.Count; i++) { resultOpd.Add(leftOpd[i] || rightOpd[i]); }
                    break;
                case '→':
                    for (int i = 0; i < leftOpd.Count; i++) { resultOpd.Add(!leftOpd[i] || rightOpd[i]); }
                    break;
                case '↔':
                    for (int i = 0; i < leftOpd.Count; i++) { resultOpd.Add(leftOpd[i] == rightOpd[i]); }
                    break;
            }

            return resultOpd;
        }

		//Це використовується для дублювання полів даних при введенні нових змінних
		//Так уникаються надмірні розрахунки, оскільки ці зайві дані просто дублюються
		private int AddVar(string keyVar, int varCount)
        {
			//Дублюємо надлишкові дані
			foreach (var i in EvalPlan)
            {
                i.Value.fieldResult.AddRange(i.Value.fieldResult);
            }

			//Додаємо нову змінну
			//Присвоюємо TRUE одній частині та FALSE іншій еквівалентній частині
			for (int i = 0; i < Math.Pow(2, varCount); i++)
            {
                EvalPlan[keyVar].fieldResult.Add(true);
            }
            for (int i = 0; i < Math.Pow(2, varCount); i++)
            {
                EvalPlan[keyVar].fieldResult.Add(false);
            }

            return ++varCount;
		}

        //Обчислює запит і повертає таблицю результатів у форматі DataView
		public dynamic EvaluateQuery()
        {
			//Отримати план оцінки
			EvalPlan = FindEvalPlan();

			//Якщо потрібно додати нову змінну, використовуйте addVar ()
			//В іншому випадку виконайте операцію
			int varCount = 0;
            foreach (var field in EvalPlan)
            {
                if (field.Key.Length == 1) { varCount = AddVar(field.Key, varCount); } 
                else { field.Value.fieldResult = ExecOp(field.Value.fieldOpr != '¬' ? EvalPlan[field.Value.leftOpd].fieldResult : null, EvalPlan[field.Value.rightOpd].fieldResult, field.Value.fieldOpr); }
            }

			//Сформувати таблицю за результатами оцінки
			//Повернути їх як DataView
			return GenerateTable();
        }

       
        

		// Генерує таблицю і також сортує її
        public virtual dynamic GenerateTable()
        {
            DataTable truthTable = new DataTable();

			// Створюємо порожні стовпці як підставки для таблиці
			// Використовуємо ключ як заголовок рядка
			
			foreach (var column in EvalPlan)
            {
				truthTable.Columns.Add(column.Key.ToString().Replace("¬( ¬", "¬(") + "\b");
            }

			// перебираємо дані та додаємо у таблицю
			for (int i = 0; i < EvalPlan.ElementAt(0).Value.fieldResult.Count; i++)
            {
                DataRow tableRow = truthTable.NewRow();

                for (int j = 0; j < EvalPlan.Count; j++)
                {
					tableRow[j] = EvalPlan.ElementAt(j).Value.fieldResult[i] ? '1' : '0';
				}

                truthTable.Rows.Add(tableRow);
            }

			// Створити подання за замовчуванням та відсортувати його на основі стовпців зі змінними за зростанням
			// Неперервний підрахунок зменшується на 2 для кожної послідовної змінної
			DataView tableView = truthTable.DefaultView;
            string tableViewSort = "";
            foreach (DataColumn x in truthTable.Columns)
            {
                if (x.ColumnName.Length == 2) { tableViewSort += x.ColumnName + " DESC , "; }
            }
            tableView.Sort = tableViewSort.Remove(tableViewSort.Length - 3, 3);

            return tableView;
        }

        // Знаходить ДКНФ
        public string FindPCNF() { return FindNormalForm(false, '∨', '∧'); }

		// Знаходить ДДНФ
		public string FindPDNF() { return FindNormalForm(true, '∧', '∨'); }

		// Ця функція використовується для пошуку ДКНФ та ДДНФ даного запиту
		// Процедура заснована на таблиці істинності
		private string FindNormalForm(bool truth, char op1, char op2)
        {
            string normalForm = "";

			// Отримаємо поле результату та поле змінних
			string resultId = "";
            Field result = null;
            List<KeyValuePair<string, Field>> variables = new List<KeyValuePair<string, Field>>();
            foreach (var i in EvalPlan)
            {
                if (resultId.Length < i.Key.Length) { result = i.Value; resultId = i.Key; }
                if (i.Key.Length == 1) { variables.Add(i); }
            }

            
            for (int i = 0; i < result.fieldResult.Count; i++)
            {
				// Якщо це дорівнює значенню істини, то додаємо новий термін
				if (result.fieldResult[i] == truth)
                {
                    normalForm += " ( ";
                    foreach (var j in variables)
                    {
						//Якщо поточне значення хибне, то додаємо оператор заперечення
						if (j.Value.fieldResult[i] == false) { normalForm += AddVarInversion(AddVar(j.Key)); } else { normalForm += AddVar(j.Key); }
                        normalForm +=" " + AddSymbol(op1) + " ";
                    }
                    //Видаляємо зайвий оператор
                    normalForm = FormSubstring(normalForm, 3);
					normalForm += " ) " + AddSymbol(op2);
                }
            }
			//Видаляємо зайвий оператор
			normalForm = FormSubstring(normalForm,2);

            return normalForm;
        }
		public virtual dynamic AddSymbol(char symbol)
		{
			return symbol;
		}
		public virtual string FormSubstring(string normalF, int length)
		{
			return normalF.Substring(0, normalF.Length - length); 
		}
		
		public virtual string AddVar(string varName)
		{
			return varName;
		}
		public virtual string AddVarInversion(string varName)
		{
			return "¬" + varName;
		}
	}
}
