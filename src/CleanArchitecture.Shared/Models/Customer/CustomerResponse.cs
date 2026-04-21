namespace CleanArchitecture.Shared.Models.Customer;

public class CustomerResponse
{
    public int Id { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerTaxCode { get; set; }
    public string? Address { get; set; }
}
