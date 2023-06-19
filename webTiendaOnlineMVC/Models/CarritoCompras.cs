namespace webTiendaOnlineMVC.Models
{
    public class CarritoCompras
    {
        public int CarritoId { get; set; }
        public int UsuarioId { get; set; }
        public DateTime FechaCreacion { get; set; }
        public List<DetalleCarrito> Detalles { get; set; }
    }
}
