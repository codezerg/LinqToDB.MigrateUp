using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace LinqToDB.MigrateUp.Linq
{
    class ExpressionCloner : ExpressionVisitor
    {
        public T Clone<T>(T expression) where T : Expression
        {
            return (T)Visit(expression);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            var left = Visit(node.Left);
            var right = Visit(node.Right);
            return Expression.MakeBinary(node.NodeType, left, right, node.IsLiftedToNull, node.Method);
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            var operand = Visit(node.Operand);
            return Expression.MakeUnary(node.NodeType, operand, node.Type, node.Method);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            // Simply construct a new constant expression with the same value
            return Expression.Constant(node.Value);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var expression = Visit(node.Expression);
            return Expression.MakeMemberAccess(expression, node.Member);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var obj = Visit(node.Object);
            var args = Visit(node.Arguments);
            return Expression.Call(obj, node.Method, args);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return Expression.Parameter(node.Type, node.Name);
        }

        public override Expression Visit(Expression node)
        {
            return base.Visit(node);
        }
    }
}
