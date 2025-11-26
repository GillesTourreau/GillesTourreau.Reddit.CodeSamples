namespace CompositeValidation
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var entity = new Entity();

            // Compose the validators :
            // - Validator1
            // - Composite
            //   - Validator2
            //   - Validator3
            var validators = new CompositeValidator(
                [
                    new Rule1Validator(),
                    new CompositeValidator(
                        [
                            new Rule2Validator(),
                            new Rule3Validator(),
                        ])
                ]);

            var result = validators.Validate(entity);
        }
    }

    public class Entity { }

    // Interface for all validators
    public interface IValidator
    {
        bool Validate(Entity entity);
    }

    // Implementation of validators for each rule.
    public class Rule1Validator : IValidator
    {
        public bool Validate(Entity entity)
        {
            // Business validation here for the Rule #1

            throw new NotImplementedException();
        }
    }

    public class Rule2Validator : IValidator
    {
        public bool Validate(Entity entity)
        {
            // Business validation here for the Rule #2

            throw new NotImplementedException();
        }
    }

    public class Rule3Validator : IValidator
    {
        public bool Validate(Entity entity)
        {
            // Business validation here for the Rule #3

            throw new NotImplementedException();
        }
    }

    // Composite validator to combine other validators
    // and other composite validator if need.
    public class CompositeValidator : IValidator
    {
        private readonly IReadOnlyList<IValidator> validators;

        public CompositeValidator(IReadOnlyList<IValidator> validators)
        {
            this.validators = validators;
        }

        public bool Validate(Entity entity)
        {
            foreach (var validator in this.validators)
            {
                if (!validator.Validate(entity))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
