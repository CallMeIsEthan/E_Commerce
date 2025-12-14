namespace E_Commerce.Dto
{
    public class BrandUpdateDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string LogoUrl { get; set; }

        public bool IsActive { get; set; }

        public int DisplayOrder { get; set; }
        public bool IsDeleted { get; set; }
    }
}

