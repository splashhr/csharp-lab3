namespace Lab3
{
    public struct Toy
    {
        public Toy(string name, decimal price, int minAge, int maxAge)
        {
            Name = name;
            Price = price;
            MinAge = minAge;
            MaxAge = maxAge;
        }

        public string Name
        {
            get; set;
        }

        public decimal Price
        {
            get; set;
        }

        public int MinAge
        {
            get; set;
        }

        public int MaxAge
        {
            get; set;
        }
    }
}
