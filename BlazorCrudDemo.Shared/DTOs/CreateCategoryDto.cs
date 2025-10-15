namespace BlazorCrudDemo.Shared.DTOs
{
    public class CreateCategoryDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
