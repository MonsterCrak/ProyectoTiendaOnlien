namespace webTiendaOnlineMVC.Models
{
    public class Venta
    {
        public int VentaId { get; set; }
        public int UsuarioId { get; set; }
        public DateTime FechaVenta { get; set; }
        public int MetPagoId { get; set; }
        public int EstadoId { get; set; }
        public decimal Total { get; set; }
    }
}
