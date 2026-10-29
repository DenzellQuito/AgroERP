namespace AgroERP.Models.Maestros;

public class Articulo
{
    public int Id { get; set; }
    public string Codigo { get; set; } = null!;          // único
    public string Nombre { get; set; } = null!;
    public string Tipo { get; set; } = null!;
    public string UnidadMedida { get; set; } = null!;    // Jaba, Kg, Litro, Saco...
}

public class Proveedor
{
    public int Id { get; set; }
    public string Ruc { get; set; } = null!;             // único
    public string RazonSocial { get; set; } = null!;
    public int NivelConfiabilidad { get; set; }          // 1..5
}

public class Cliente
{
    public int Id { get; set; }
    public string RucDni { get; set; } = null!;
    public string RazonSocial { get; set; } = null!;
    public decimal LimiteCredito { get; set; }           // (18,2)
}

public class ParametroInventario
{
    public int Id { get; set; }
    public int ArticuloId { get; set; }                  // único: 1 por artículo
    public Articulo Articulo { get; set; } = null!;
    public decimal StockSeguridad { get; set; }          // (18,4)
    public decimal LoteEconomico { get; set; }           // (18,4)
}
