namespace TaxManager.Domain.Entities;

public class Municipality
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;

    private readonly List<TaxRecord> _taxRecords = [];
    public IReadOnlyCollection<TaxRecord> TaxRecords => _taxRecords.AsReadOnly();

    private Municipality() { }

    public Municipality(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Municipality name is required.", nameof(name));
        }

        Id = Guid.NewGuid();
        Name = name.Trim();
    }

    public void AddTaxRecord(TaxRecord taxRecord) => _taxRecords.Add(taxRecord);
}
