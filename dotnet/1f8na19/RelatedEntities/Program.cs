namespace RelatedEntities
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            var _context = new Context();
            var _currentUser = new User();
            var command = new { RelatedEntity1Id = 1, RelatedEntity2Id = 2, RelatedEntity3Id = 3 };

            var entitiesExists = _context.RelatedEntities1
                .Where(e => e.Id == command.RelatedEntity1Id && e.UserId == _currentUser.UserId)
                .Select(_ => new
                {
                    RelatedEntities2Exists = _context.RelatedEntities2.Any(e => e.Id == command.RelatedEntity2Id && e.UserId == _currentUser.UserId),
                    RelatedEntities3Exists = _context.RelatedEntities3.Any(e => e.Id == command.RelatedEntity3Id && e.UserId == _currentUser.UserId),
                });

            var result = entitiesExists.FirstOrDefault();

            if (result is null)
            {
                throw new Exception($"There's no related-entity1 with id: {command.RelatedEntity1Id}");
            }

            if (!result.RelatedEntities2Exists)
            {
                throw new Exception($"There's no related-entity2 with id: {command.RelatedEntity2Id}");
            }

            if (!result.RelatedEntities3Exists)
            {
                throw new Exception($"There's no related-entity3 with id: {command.RelatedEntity3Id}");
            }
        }
    }

    internal class  Context
    {
        public List<User> RelatedEntities1 { get; }

        public List<User> RelatedEntities2 { get; }

        public List<User> RelatedEntities3 { get; }
    }

    internal class User
    {
        public int UserId { get; set; }

        public int Id { get; set; }
    }
}
