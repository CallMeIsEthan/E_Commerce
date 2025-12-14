namespace E_Commerce.Dto
{
    public class BrandCreateDto
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string LogoUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public int DisplayOrder { get; set; } = 0;
        public bool IsDeleted { get; set; }
    }
}

