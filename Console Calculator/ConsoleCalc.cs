using System;
using System.Collections.Generic;
using System.Linq;

namespace Console_Calculator {
    internal class Operands {
        private enum Constants {
            Euler = 'e',
            Pi = 'π',
            PiAlt = 'p'
        }

        internal static bool IsEulerConst(string c) => ((char)Constants.Euler).ToString().Equals(c);

        internal static bool IsPiConst(string c) => 
            ((char)Constants.Pi).ToString().Equals(c) ||
            ((char)Constants.PiAlt).ToString().Equals(c);
        
        private readonly Stack<double> _operands;
        private readonly bool _verbose;
        
        internal Operands(int capacity, bool verbose = false){
            _operands = new Stack<double>(capacity);
            _verbose = verbose;
        }
        
        internal double Peek() => _operands.Peek();
        
        internal double Pop(){
            var op = _operands.Pop();
            if (_verbose) { Console.WriteLine($"Operands stack popped: {op}"); }
            return op;
        }
        
        internal void Push(double newOperand){
            _operands.Push(newOperand);
            if (_verbose) { Console.WriteLine($"Operands stack pushed: {_operands.Peek()}"); }
        }
        
        internal bool CanPerformBinaryOperation() => _operands.Count >= 2;
    }
    
    internal class Operators {
        private enum Ops {
            OpenParenthesis = '(',
            CloseParenthesis = ')',
            Exponentiation = '^',
            Logarithm = 'v',
            LogarithmAlt = 'V',
            Multiplication = '*',
            MultiplicationAlt = '×',
            MultiplicationAlt0 = 'x',
            MultiplicationAlt1 = 'X',
            Division = '/',
            DivisionAlt = '÷',
            Modulo = '%',
            Addition = '+',
            Subtraction = '-',
            Positive = Addition,
            Negative = Subtraction,
            Factorial = '!',
        }

        internal static bool IsOpenParenthesisOp(string op) => ((char)Ops.OpenParenthesis).ToString().Equals(op);

        internal static bool IsCloseParenthesisOp(string op) => ((char)Ops.CloseParenthesis).ToString().Equals(op);

        internal static bool IsExponentiationOp(string op) => ((char)Ops.Exponentiation).ToString().Equals(op);

        internal static bool IsLogarithmOp(string op) =>
            ((char)Ops.Logarithm).ToString().Equals(op) ||
            ((char)Ops.LogarithmAlt).ToString().Equals(op);

        internal static bool IsMultiplicationOp(string op) =>
                ((char)Ops.Multiplication).ToString().Equals(op) ||
                ((char)Ops.MultiplicationAlt).ToString().Equals(op) ||
                ((char)Ops.MultiplicationAlt0).ToString().Equals(op) ||
                ((char)Ops.MultiplicationAlt1).ToString().Equals(op);
        
        internal static bool IsDivisionOp(string op) =>
                ((char)Ops.Division).ToString().Equals(op) ||
                ((char)Ops.DivisionAlt).ToString().Equals(op);
        
        internal static bool IsModuloOp(string op) => ((char)Ops.Modulo).ToString().Equals(op);
        
        internal static bool IsAdditionOp(string op) => ((char)Ops.Addition).ToString().Equals(op);
        
        internal static bool IsSubtractionOp(string op) => ((char)Ops.Subtraction).ToString().Equals(op);
        
        internal static bool IsPositiveOp(string op) => ((char)Ops.Positive).ToString().Equals(op);
        
        internal static bool IsNegativeOp(string op) => ((char)Ops.Negative).ToString().Equals(op);
        
        internal static bool IsFactorialOp(string op) => 
            ((char)Ops.Factorial).ToString().Equals(op);
        
        internal static bool IsBinaryOp(string op) =>
                IsExponentiationOp(op) ||
                IsLogarithmOp(op) ||
                IsMultiplicationOp(op) ||
                IsModuloOp(op) ||
                IsDivisionOp(op) ||
                IsAdditionOp(op) ||
                IsSubtractionOp(op);
        
        internal static string OpenParenthesisOp() => ((char)Ops.OpenParenthesis).ToString();
        
        internal static string CloseParenthesisOp() => ((char)Ops.CloseParenthesis).ToString();
        
        internal static string MultiplicationOp() => ((char)Ops.Multiplication).ToString();
        
        private static readonly Dictionary<string, int> OpPrecedence = new Dictionary<string, int>();
        
        private readonly Stack<string> _operators;
        private readonly Stack<bool> _makeNegatives;
        
        private readonly bool _verbose;
        
        internal Operators(int capacity, bool verbose){
            _operators = new Stack<string>(capacity);
            _makeNegatives = new Stack<bool>(capacity);
            _verbose = verbose;
            
            if (OpPrecedence.Count > 0) { return; }
            
            var opType = typeof(Ops);
            Array names = Enum.GetNames(opType);

            { int precedence;
                foreach (string name in names)
                {
                    precedence = int.MinValue;
                    if (
                        name.StartsWith(Ops.Addition.ToString()) ||
                        name.StartsWith(Ops.Subtraction.ToString())
                    ) {
                        precedence = 12;
                    } else
                    if (
                        name.StartsWith(Ops.Modulo.ToString()) ||
                        name.StartsWith(Ops.Division.ToString()) ||
                        name.StartsWith(Ops.Multiplication.ToString())
                    ) {
                        precedence = 13;
                    } else
                    if (
                        name.StartsWith(Ops.Exponentiation.ToString()) ||
                        name.StartsWith(Ops.Logarithm.ToString())
                    ) {
                        precedence = 14;
                    } else
                    if (
                        name.StartsWith(Ops.Positive.ToString()) ||
                        name.StartsWith(Ops.Negative.ToString()) ||
                        name.StartsWith(Ops.Factorial.ToString())
                    ) {
                        precedence = 16;
                    } else
                    if (
                        name.StartsWith(Ops.OpenParenthesis.ToString()) ||
                        name.StartsWith(Ops.CloseParenthesis.ToString())
                    ) {
                        precedence = 17;
                    }

                    if (precedence < 0) { continue; } 

                    var op = (Ops) Enum.Parse(opType, name);
                    OpPrecedence.Add(((char) op).ToString(), precedence);
            }} 
        }
        
        internal string Peek() => _operators.Peek();
        
        internal string Pop(){
            var op = _operators.Pop();
            if (_verbose) { Console.WriteLine($"Operators stack popped: {op}"); }
            return op;
        }

        internal bool Push(string newOperator){
            if (
                newOperator == null || 
                IsCloseParenthesisOp(newOperator) || 
                ( 
                    _operators.Any() && 
                    !IsOpenParenthesisOp(Peek()) && 
                    ( 
                        !IsExponentiationOp(Peek()) || 
                        !IsExponentiationOp(newOperator) 
                    ) && 
                    !HasGreaterPrecedence(newOperator) 
                )
            ) { 
                if (_verbose) { Console.WriteLine($"could not push {newOperator} onto Operators stack"); }
                return false;
            }
            
            _operators.Push(newOperator);
            if (_verbose) { Console.WriteLine($"Operators stack pushed: {Peek()}"); }
            return true;
        }
        
        private bool HasGreaterPrecedence(string newOperator){
            if (
                IsCloseParenthesisOp(newOperator)
            ) {
                Console.WriteLine($"{CloseParenthesisOp()} has a high precedence but should not be pushed, instead evaluate using the operator stack until a '{OpenParenthesisOp()}'");
                return false;
            }

            if (
                !IsOpenParenthesisOp(newOperator) &&
                !IsBinaryOp(newOperator)
            ) {
                Console.WriteLine($"operator {newOperator} is not valid");
                return false;
            }

            var topOperator = Peek();
            if (
                OpPrecedence[newOperator] <= OpPrecedence[topOperator]
            ) {
                if (_verbose) { Console.WriteLine($"{newOperator} <= {topOperator}"); }
                return false;
            }
                
            if (_verbose) { Console.WriteLine($"{newOperator} > {topOperator}"); }
            return true;

        }
        
        internal bool IsEmpty(){
            var res = _operators.Count == 0;

            if (_verbose)
            {
                Console.WriteLine(
                    res ?
                        "Operators stack is empty" :
                        "Operators stack is NOT empty"
                    );
            }

            return res;
        }
        
        internal void ToggleNegation(){
            var makeNegative = MakeNegative();
            _makeNegatives.Push(!makeNegative);
        }
        
        internal bool MakeNegative() =>
                _makeNegatives.Count > 0 &&
                _makeNegatives.Pop();
    }
    
    public class ExpressionEvaluator{
        private readonly Operands _operands;
        private readonly Operators _operators;
        
        private readonly string _expression;
        private readonly bool _verbose;
        
        public ExpressionEvaluator(string expression, bool verbose = false){
            _verbose = verbose;
            if (_verbose){ Console.WriteLine($"Expression: {expression}"); }
            _expression = expression.Replace(" ", ""); // space is just formatting
            if (_verbose){ Console.WriteLine($"Evaluate Expression: {_expression}"); }
            
            _operands = new Operands(_expression.Length, _verbose);
            _operators = new Operators(_expression.Length, _verbose);
        }
        
        private double Subtraction(){
            var right = _operands.Pop();
            var left = _operands.Pop();
            var ret = left - right;
            if (_verbose) { Console.WriteLine($"{left} {_operators.Peek()} {right} = {ret}"); }
            return ret;
        }
        
        private double Addition(){
            var right = _operands.Pop();
            var left = _operands.Pop();
            var ret = left + right;
            if (_verbose) { Console.WriteLine($"{left} {_operators.Peek()} {right} = {ret}"); }
            return ret;
        }
        
        private double Modulo(){
            var right = _operands.Pop();
            var left = _operands.Pop();
            var ret = left % right;
            if (_verbose) { Console.WriteLine($"{left} {_operators.Peek()} {right} = {ret}"); }
            return ret;
        }
        
        private double Division(){
            var right = _operands.Pop();
            var left = _operands.Pop();
            var ret = left / right;
            if (_verbose) { Console.WriteLine($"{left} {_operators.Peek()} {right} = {ret}"); }
            return ret;
        }
        
        private double Multiplication(){
            var right = _operands.Pop();
            var left = _operands.Pop();
            var ret = left * right;
            var op = !_operators.IsEmpty() ? _operators.Peek() : Operators.MultiplicationOp(); // deal with implicit multiplication
            if (_verbose) { Console.WriteLine($"{left} {op} {right} = {ret}"); }
            return ret;
        }
        
        private double Logarithm(){
            var right = _operands.Pop();
            var left = _operands.Pop();
            var ret = Math.Log(left, right);
            if (_verbose) { Console.WriteLine($"{left} {_operators.Peek()} {right} = {ret}"); }
            return ret;
        }
        
        private double Exponentiation(){
            var right = _operands.Pop();
            var left = _operands.Pop();
            var ret = Math.Pow(left, right);
            if (_verbose) { Console.WriteLine($"{left} {_operators.Peek()} {right} = {ret}"); }
            return ret;
        }
        
        private int EvaluatePart(string newOperator = null) {
            var pushedOperator = false;

            
            if (
                newOperator != null &&
                !Operators.IsCloseParenthesisOp(newOperator)
            ) {
                pushedOperator = _operators.Push(newOperator);
                if (pushedOperator) { return 0; }  
            }

            { string topOperator = null; double result;
            while (
                !_operators.IsEmpty() &&
                (
                    (newOperator != null && !Operators.IsCloseParenthesisOp(newOperator) && !pushedOperator) || 
                    Operators.IsCloseParenthesisOp(newOperator) || 
                    newOperator == null
                )
            ) {
                topOperator = _operators.Peek();

                if (Operators.IsOpenParenthesisOp(topOperator)) {
                    var matching = Operators.IsCloseParenthesisOp(newOperator);
                    var prefix = !matching ? "no " : "";
                    if (_verbose) { Console.WriteLine($"'{Operators.OpenParenthesisOp()}' found with {prefix}matching '{Operators.CloseParenthesisOp()}'"); }
                    _operators.Pop();
                    return !matching ? -1 : 0;
                }
                
                if (!_operands.CanPerformBinaryOperation()) {
                    return -1;
                }
                
                if (
                    Operators.IsSubtractionOp(topOperator)
                ) {
                    result = Subtraction();
                } else
                if (
                    Operators.IsAdditionOp(topOperator)
                ) {
                    result = Addition();
                } else
                if (
                    Operators.IsModuloOp(topOperator)
                ) {
                    if (_operands.Peek() == 0) {
                        Console.WriteLine("avoided divide by zero");
                        return -1;
                    }
                    result = Modulo();
                } else
                if (
                    Operators.IsDivisionOp(topOperator)
                ) {
                    if (_operands.Peek() == 0) {
                        Console.WriteLine("avoided divide by zero");
                        return -1;
                    }
                    result = Division();
                } else
                if (
                    Operators.IsMultiplicationOp(topOperator)
                ) {
                    result = Multiplication();
                } else
                if (
                    Operators.IsLogarithmOp(topOperator)
                ) {
                    result = Logarithm();
                } else
                if (
                    Operators.IsExponentiationOp(topOperator)
                ) {
                    result = Exponentiation();
                } else {
                    Console.WriteLine($"top operator {topOperator} is not valid");
                    return -1;
                }
                
                _operators.Pop();
                _operands.Push(result);

                if (
                    newOperator != null &&
                    !Operators.IsCloseParenthesisOp(newOperator)
                ) {
                    pushedOperator = _operators.Push(newOperator);
                }
            }} 
            
            if (Operators.IsCloseParenthesisOp(newOperator)) {
                Console.WriteLine($"'{Operators.CloseParenthesisOp()}' found with no matching '{Operators.OpenParenthesisOp()}'");
                return -1;
            }
            return 0;
        }
        
        private bool EvaluateUntilOperatorPush(string newOperator) => EvaluatePart(newOperator) == 0;
        
        private bool EvaluateUntilOpenParenthesis() => EvaluatePart(Operators.CloseParenthesisOp()) == 0;
        
        private bool EvaluateUntilDone() => EvaluatePart() == 0;
        
        public double? Evaluate(){
            if (string.IsNullOrWhiteSpace(_expression)) {
                Console.WriteLine("expression invalid: empty");
                return null;
            }
            
            var pushedOperator = true;
            for (var i = 0; i < _expression.Length; ++i) {
                var c = _expression[i].ToString();
                if (_verbose) { Console.WriteLine($"Evaluate: {c}"); }
                if (
                    pushedOperator &&
                    Operators.IsPositiveOp(c) &&
                    i + 1 < _expression.Length
                ) {   
                    continue;
                }
                if ( 
                    pushedOperator &&
                    Operators.IsNegativeOp(c) &&
                    i + 1 < _expression.Length
                ) {
                    _operators.ToggleNegation();
                } else
                if (
                    !pushedOperator &&
                    Operators.IsFactorialOp(c)
                ) {
                    double product = 1;
                    long j = (long)_operands.Peek();
                    if (j < 0) {
                        j = Math.Abs(j);
                        product *= -1;
                    }
                    for (; j > 1; --j) {
                        product *= j;
                    }
                    _operands.Pop();
                    _operands.Push(product);
                } else
                if (
                    Operands.IsEulerConst(c)
                ) {
                    pushedOperator = false;
                    _operands.Push(Math.E);
                } else
                if (
                    Operands.IsPiConst(c)
                ) {
                    pushedOperator = false;
                    _operands.Push(Math.PI);
                } else
                if (
                    c[0] == 'S' || c[0] == 's'
                ) {
                    //Console.WriteLine("sin");
                    if (
                        ( _expression[1] == 'I' || _expression[1] == 'i' ) &&
                        ( _expression[2] == 'N' || _expression[2] == 'n' )
                    ) {
                        // sin
                        Console.WriteLine("sin");

                        pushedOperator = false;
                        i += 2;
                    }
                } else
                if (
                    c[0] == 'C' || c[0] == 'c'
                ) {
                    if (
                        ( c[1] == 'O' || c[1] == 'o' ) &&
                        ( c[2] == 'S' || c[2] == 's' )
                    ) {
        
                        Console.WriteLine("cos");

                        pushedOperator = false;
                        i += 2;
                    }
                } else
                if (
                    c[0] == 'T' || c[0] == 't'
                ) {
                    if (
                        ( c[1] == 'A' || c[1] == 'a' ) &&
                        ( c[2] == 'N' || c[2] == 'n' )
                    ) {
                        Console.WriteLine("tan");

                        pushedOperator = false;
                        i += 2;
                    }
                } else
                if (
                    Char.GetNumericValue(c[0]) != -1.0 ||
                    c == "."
                ) {    
                    var parsedNum = false;

                    for (var j = _expression.Length - i; j > 0; --j) { 
                        if (
                            !(parsedNum = double.TryParse(_expression.Substring(i, j), out var num))
                        ) { continue; } 
                        
                        
                        i += j - 1;
                        pushedOperator = false;
                        num = _operators.MakeNegative() ? -num : num;
                        _operands.Push(num);
                        break;
                    }
                    
                    if (!parsedNum) { return null; }
                } else
                if (
                    pushedOperator &&
                    Operators.IsOpenParenthesisOp(c)
                ) { 
                    if (
                        _operators.MakeNegative()
                    ) { 
                        _operands.Push(-1);
                        _operators.Push(Operators.MultiplicationOp());
                    }
                    pushedOperator = EvaluateUntilOperatorPush(c);
                    if (!pushedOperator) { return null; }
                } else
                if (!pushedOperator) { 
                    if (Operators.IsOpenParenthesisOp(c)) { 
                            if (!EvaluateUntilOperatorPush(Operators.MultiplicationOp())) { return null; }
                            
                            pushedOperator = EvaluateUntilOperatorPush(c);
                            if (!pushedOperator) { return null; }
                    } else
                    if (
                        Operators.IsBinaryOp(c)
                    ) {
                        pushedOperator = EvaluateUntilOperatorPush(c);
                        if (!pushedOperator) { return null; }
                    } else
                    if (
                        Operators.IsCloseParenthesisOp(c)
                    ) {
                        pushedOperator = false;
                        if (!EvaluateUntilOpenParenthesis()) { return null; }
                    } else {
                        Console.WriteLine($"character {c} is not a valid binary operator");
                        return null;
                    }
                }
                else {
                    Console.WriteLine($"character {c} is not a valid operand or valid unary operator in this context");
                    return null;
                }
            }
            
            var done = EvaluateUntilDone();
            if (!done) { 
                return null;
            }
            
            while (_operands.CanPerformBinaryOperation()) {    
              _operands.Push(Multiplication());
            } 
            
            return _operands.Peek();
        }
        
        public void Print(){
            Console.WriteLine($"{_expression} = {_operands.Peek()}");
        }
    }
    
    public class Program{
        private const bool Test = false;
        private static string _expression;
        private static double? _expected = null;

        public static void Main(string[] args){
            _expression = Console.ReadLine();
            double parsedExpected = 0;
            var bParsed = false;
            if (
                Test ||
                string.IsNullOrWhiteSpace(_expression) ||
                (bParsed = double.TryParse(Console.ReadLine(), out parsedExpected))
            ) {
                _expected = bParsed ? (double?)parsedExpected : null;
                RegressionTest();
            } else {
                Calculate();
            }

            Console.ReadLine();
        }

        private static void Calculate(){
            var ex = new ExpressionEvaluator(_expression.Trim(), !Test);
            var res = ex.Evaluate();
            if (res == null) {
                Console.WriteLine("error while evaluating expression");
                return;
            }
            
            ex.Print();
        }


        private static void RegressionTest(){
            Console.WriteLine("Begin regression test...");

            var testData = new List<Tuple<string, double?>> {
                new Tuple<string, double?>("-12 * 1 + -721 * (15 +51)", -47598),
                new Tuple<string, double?>("(5+3)*6", 48),
                new Tuple<string, double?>("(((2+3))*(4+2))", 30),
                new Tuple<string, double?>("2*3/5", 1.2),
                new Tuple<string, double?>("2^3*2", 16),
                new Tuple<string, double?>("3-3+3*3/3^3", 1.0 / 3.0),
                new Tuple<string, double?>("2^2^3", 256),
                new Tuple<string, double?>("--5", 5),
                new Tuple<string, double?>("-(-5)", 5),
                new Tuple<string, double?>("4(5)", 20),
                new Tuple<string, double?>("(5)6", 30),
                new Tuple<string, double?>("100 v 10", 2),
                new Tuple<string, double?>("p", Math.PI/*3.14159265358979*/),
                new Tuple<string, double?>("π", Math.PI/*3.14159265358979*/),
                new Tuple<string, double?>("e", Math.E/*2.71828182845905*/),
                new Tuple<string, double?>("5×6÷10x3X2", 18),
                new Tuple<string, double?>("e^π", Math.Pow(Math.E, Math.PI)/*23.1406926327793*/),
                new Tuple<string, double?>("-(5+8)", -13),
                new Tuple<string, double?>("(-42) + (+24) + 12 + 8 - (-4)", 6),
                new Tuple<string, double?>("21 + 40 - (+9) + 413 + (-21) + 4 + 3", 451),
                new Tuple<string, double?>("5.92 - 27 + 19 - 37.1 + 27 - 25", -37.18),
                new Tuple<string, double?>("6 ÷ 2 (1+2)", 9),
                new Tuple<string, double?>("1+1", 2),
                new Tuple<string, double?>("-1+-1", -2),
                new Tuple<string, double?>("1.5*2", 3),
                new Tuple<string, double?>("-2^3", -8),
                new Tuple<string, double?>("2*(1+1)", 4),
                new Tuple<string, double?>("1+2(1+1)^3", 17),
                new Tuple<string, double?>("2(2(2(2(1+1))))", 32),
                new Tuple<string, double?>("(3^2)^.5", 3),
                new Tuple<string, double?>("-(2^2)--2", -2),
                new Tuple<string, double?>("2((2+2)(2+1)(1+1)(1+0))*2", 96),
                new Tuple<string, double?>("π*2^2", Math.PI * 4),
                new Tuple<string, double?>("2^1^3", 2),
                new Tuple<string, double?>("1+(2*(2+1)+2-(3*2)+1)", 4),
                new Tuple<string, double?>("-2^4", 16),
                new Tuple<string, double?>("5!", 120),
                new Tuple<string, double?>("0!", 1),
                new Tuple<string, double?>("1!", 1),
                new Tuple<string, double?>("5.5!", 120),
                new Tuple<string, double?>("3!!", 720),
                new Tuple<string, double?>("4!-3!", 18),
            };

            if (
                !string.IsNullOrWhiteSpace(_expression) &&
                _expected != null
            ) {
                testData.Add(new Tuple<string, double?>(_expression, _expected));
            }
            
            var passed = 0;
            var failed = 0;
            foreach (var testDatum in testData) {
                var result = TestCase(
                    testDatum.Item1, 
                    testDatum.Item2  
                );
                if (result) { ++passed; }
                else { ++failed; }
            }
            
            Console.WriteLine("... End regression test");
            Console.WriteLine("Report:");
            Console.WriteLine($"Total: {passed + failed}");
            if (passed > 0) { Console.WriteLine($"Passed: {passed}"); }
            if (failed > 0) { Console.WriteLine($"Failed: {failed}"); }
        }

        private static bool TestCase(string expression, double? expected){
            var actual = new ExpressionEvaluator(expression).Evaluate();
            
            Console.WriteLine($"{expression} = {actual}");
     
            if (expected != actual) {
                Console.WriteLine($"failed, expected: {expected}");
                return false;
            }

            Console.WriteLine("passed");
            return true;
        }
        private static bool NearEqual(double? expected, double? actual, double epsilon = double.Epsilon/* 9 × 10^-16 */){
            if (expected == null && actual == null) { return true; }
            if (expected == null || actual == null) { return false; }
            
            var absExpected = Math.Abs((double)expected);
            var absActual = Math.Abs((double)actual);
            var magnitude = Math.Abs((double)(expected - actual));
        
            if (expected == actual) { return true; }
            if (
                expected == 0 || 
                actual == 0 || 
                magnitude < double.Epsilon 
            ) { return magnitude < epsilon; }  
            return magnitude / (absExpected + absActual) < epsilon; 
        }
    }
}