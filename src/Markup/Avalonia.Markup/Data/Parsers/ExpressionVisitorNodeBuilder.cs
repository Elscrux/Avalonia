﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Avalonia.Markup.Data.Parsers
{
    class ExpressionVisitorNodeBuilder : ExpressionVisitor
    {
        private static PropertyInfo AvaloniaObjectIndexer;

        private readonly bool enableDataValidation;

        static ExpressionVisitorNodeBuilder()
        {
            AvaloniaObjectIndexer = typeof(AvaloniaObject).GetProperty("Item", new[] { typeof(AvaloniaProperty) });
        }

        public List<ExpressionNode> Nodes { get; }

        public ExpressionVisitorNodeBuilder(bool enableDataValidation)
        {
            this.enableDataValidation = enableDataValidation;
            Nodes = new List<ExpressionNode>();
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node.NodeType != ExpressionType.Not || node.Type != typeof(bool))
            {
                throw new ExpressionParseException(0, $"Invalid unary operation {node.NodeType} in binding expression");
            }

            Nodes.Add(new LogicalNotNode());

            return base.VisitUnary(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            Nodes.Add(new PropertyAccessorNode(node.Member.Name, enableDataValidation));
            return base.VisitMember(node);
        }

        protected override Expression VisitIndex(IndexExpression node)
        {
            if (node.Indexer == AvaloniaObjectIndexer)
            {
                var property = GetArgumentExpressionValue<AvaloniaProperty>(node.Arguments[0]);
                Nodes.Add(new PropertyAccessorNode($"{property.OwnerType.Name}.{property.Name}", enableDataValidation));
            }
            else
            {
                Nodes.Add(new IndexerExpressionNode(node));
            }

            return node;
        }

        private T GetArgumentExpressionValue<T>(Expression expr)
        {
            try
            {
                return Expression.Lambda<Func<T>>(expr).Compile(preferInterpretation: true)();
            }
            catch (InvalidOperationException ex)
            {
                throw new ExpressionParseException(0, "Unable to parse indexer value.", ex);
            }
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node.NodeType == ExpressionType.ArrayIndex)
            {
                return base.VisitBinary(node);
            }
            throw new ExpressionParseException(0, $"Invalid expression type in binding expression: {node.NodeType}.");
        }

        protected override Expression VisitBlock(BlockExpression node)
        {
            throw new ExpressionParseException(0, $"Invalid expression type in binding expression: {node.NodeType}.");
        }

        protected override CatchBlock VisitCatchBlock(CatchBlock node)
        {
            throw new ExpressionParseException(0, $"Catch blocks are not allowed in binding expressions.");
        }

        protected override Expression VisitConditional(ConditionalExpression node)
        {
            throw new ExpressionParseException(0, $"Invalid expression type in binding expression: {node.NodeType}.");
        }

        protected override Expression VisitDynamic(DynamicExpression node)
        {
            throw new ExpressionParseException(0, $"Dynamic expressions are not allowed in binding expressions.");
        }

        protected override ElementInit VisitElementInit(ElementInit node)
        {
            throw new ExpressionParseException(0, $"Element init expressions are not valid in a binding expression.");
        }

        protected override Expression VisitGoto(GotoExpression node)
        {
            throw new ExpressionParseException(0, $"Goto expressions not supported in binding expressions.");
        }

        protected override Expression VisitInvocation(InvocationExpression node)
        {
            throw new ExpressionParseException(0, $"Invalid expression type in binding expression: {node.NodeType}.");
        }

        protected override Expression VisitLabel(LabelExpression node)
        {
            throw new ExpressionParseException(0, $"Invalid expression type in binding expression: {node.NodeType}.");
        }

        protected override Expression VisitListInit(ListInitExpression node)
        {
            throw new ExpressionParseException(0, $"Invalid expression type in binding expression: {node.NodeType}.");
        }

        protected override Expression VisitLoop(LoopExpression node)
        {
            throw new ExpressionParseException(0, $"Invalid expression type in binding expression: {node.NodeType}.");
        }

        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            throw new ExpressionParseException(0, $"Member assignments not supported in binding expressions.");
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            throw new ExpressionParseException(0, $"Invalid expression type in binding expression: {node.NodeType}.");
        }

        protected override Expression VisitSwitch(SwitchExpression node)
        {
            throw new ExpressionParseException(0, $"Invalid expression type in binding expression: {node.NodeType}.");
        }

        protected override Expression VisitTry(TryExpression node)
        {
            throw new ExpressionParseException(0, $"Invalid expression type in binding expression: {node.NodeType}.");
        }

        protected override Expression VisitTypeBinary(TypeBinaryExpression node)
        {
            throw new ExpressionParseException(0, $"Invalid expression type in binding expression: {node.NodeType}.");
        }
    }
}
