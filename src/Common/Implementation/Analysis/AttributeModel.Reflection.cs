using System;
using System.Reflection;

namespace RestEase.Implementation.Analysis
{
    internal abstract partial class AttributeModel
    {
        // May be null, if it was declared on parameters
        public MemberInfo? DeclaringMember { get; }

        public AttributeModel(MemberInfo? declaringMember)
        {
            this.DeclaringMember = declaringMember;
        }

        public static AttributeModel<T> Create<T>(T attribute, MemberInfo? declaringMember) where T : Attribute =>
            new(attribute, declaringMember);

        public bool IsDeclaredOn(TypeModel typeModel) => typeModel.Type.Equals(this.DeclaringMember);
    }


    internal partial class AttributeModel<T> : AttributeModel where T : Attribute
    {
        public AttributeModel(T attribute, MemberInfo? declaringMember)
            : base(declaringMember)
        {
            this.Attribute = attribute;
        }
    }
}