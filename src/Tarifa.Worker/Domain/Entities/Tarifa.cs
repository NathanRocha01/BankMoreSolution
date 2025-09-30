namespace Tarifa.Worker.Domain.Entities
{
    public class TarifaE
    {
        public string IdTarifa { get; set; } = Guid.NewGuid().ToString();
        public string IdContaCorrente { get; set; }
        public string DataMovimento { get; set; } = DateTime.Now.ToString("dd/MM/yyyy");
        public decimal Valor { get; set; }
    }
}
