namespace webTiendaOnlineMVC.Models
{
    public class DetalleCarrito
    {
        public int DetalleId { get; set; }
        public int CarritoId { get; set; }
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioTotalProducto { get; set; }
        public Productos Producto { get; set; }
    }
}
