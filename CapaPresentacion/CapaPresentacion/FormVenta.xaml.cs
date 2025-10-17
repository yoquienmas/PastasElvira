using CapaDatos;
using CapaEntidad;
using CapaNegocio;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Printing;
using System.Windows.Documents;
using System.Windows.Media;

namespace CapaPresentacion
{
    public partial class FormVenta : Window
    {
        private CN_Producto cnProducto = new CN_Producto();
        private CN_Venta cnVenta = new CN_Venta();
        private CN_Cliente cnCliente = new CN_Cliente();
        private CN_MetodoPago cnMetodoPago = new CN_MetodoPago();
        private List<Producto> listaProductos;
        private ObservableCollection<ItemVenta> listaItemsVenta = new ObservableCollection<ItemVenta>();
        private int idVendedor;
        private string NombreCompleto;
        private Cliente clienteSeleccionado = null;

        public FormVenta(int idUsuario, string NombreVendedor)
        {
            InitializeComponent();
            idVendedor = idUsuario;
            NombreCompleto = NombreVendedor;

            txtVendedor.Text = NombreCompleto;
            listaItemsVenta = new ObservableCollection<ItemVenta>();
            dgvItemsVenta.ItemsSource = listaItemsVenta;
            listaProductos = new List<Producto>();

            CargarMetodosPago();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                CargarProductos();
                CargarInformacionVenta();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar formulario: {ex.Message}", "Error",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarMetodosPago()
        {
            try
            {
                var metodosPago = cnMetodoPago.Listar();

                if (metodosPago != null && metodosPago.Count > 0)
                {
                    cboMetodoPago.ItemsSource = metodosPago;
                    cboMetodoPago.SelectedValuePath = "IdMetodoPago";
                    cboMetodoPago.SelectedIndex = 0;
                }
                else
                {
                    CargarMetodosPagoPorDefecto();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar métodos de pago: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                CargarMetodosPagoPorDefecto();
            }
        }

        private void CargarMetodosPagoPorDefecto()
        {
            var metodosPorDefecto = new List<MetodoPago>
            {
                new MetodoPago { IdMetodoPago = 1, Nombre = "Efectivo", Descripcion = "Pago en efectivo", Activo = true },
                new MetodoPago { IdMetodoPago = 2, Nombre = "TarjetaDebito", Descripcion = "Pago con tarjeta de débito", Activo = true },
                new MetodoPago { IdMetodoPago = 3, Nombre = "TarjetaCredito", Descripcion = "Pago con tarjeta de crédito", Activo = true },
                new MetodoPago { IdMetodoPago = 4, Nombre = "BilleteraVirtual", Descripcion = "Pago con billetera virtual", Activo = true }
            };

            cboMetodoPago.ItemsSource = metodosPorDefecto;
            cboMetodoPago.SelectedValuePath = "IdMetodoPago";
            cboMetodoPago.SelectedIndex = 0;
        }

        private void CargarInformacionVenta()
        {
            txtVendedor.Text = NombreCompleto;
            txtFecha.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        }

        private MetodoPago ObtenerMetodoPagoSeleccionado()
        {
            return cboMetodoPago.SelectedItem as MetodoPago;
        }

        private int ObtenerIdMetodoPagoSeleccionado()
        {
            var metodoSeleccionado = ObtenerMetodoPagoSeleccionado();
            return metodoSeleccionado?.IdMetodoPago ?? 1;
        }

        private string ObtenerTextoMetodoPago(MetodoPago metodo)
        {
            if (metodo == null) return "EFECTIVO";

            return metodo.Nombre.ToUpper() switch
            {
                "EFECTIVO" => "EFECTIVO",
                "TARJETADEBITO" => "TARJETA DE DÉBITO",
                "TARJETACREDITO" => "TARJETA DE CRÉDITO",
                "BILLETERAVIRTUAL" => "BILLETERA VIRTUAL",
                _ => metodo.Nombre.ToUpper()
            };
        }

        private void cboProductos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboProductos.SelectedItem is Producto productoSeleccionado)
            {
                txtPrecioUnitario.Text = productoSeleccionado.PrecioVenta.ToString("F2");
                txtStockDisponible.Text = productoSeleccionado.StockActual.ToString();
            }
        }

        private void btnAgregar_Click(object sender, RoutedEventArgs e)
        {
            if (cboProductos.SelectedItem == null)
            {
                MessageBox.Show("Debe seleccionar un producto.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtCantidad.Text, out int cantidad) || cantidad <= 0)
            {
                MessageBox.Show("La cantidad debe ser un número entero mayor que cero.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Producto productoSeleccionado = (Producto)cboProductos.SelectedItem;

            if (productoSeleccionado.StockActual < cantidad)
            {
                MessageBox.Show($"Stock insuficiente. Solo hay {productoSeleccionado.StockActual} unidades disponibles.",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var itemExistente = listaItemsVenta.FirstOrDefault(item => item.IdProducto == productoSeleccionado.IdProducto);
            if (itemExistente != null)
            {
                MessageBox.Show($"Error: El producto '{productoSeleccionado.Nombre}' ya existe en el carrito.\n\n" +
                               $"Si desea modificar la cantidad, elimine el producto actual y agréguelo nuevamente " +
                               $"con la cantidad deseada, o modifique la cantidad directamente en la lista.",
                               "Producto ya agregado",
                               MessageBoxButton.OK,
                               MessageBoxImage.Warning);
                return;
            }
            else
            {
                ItemVenta nuevoItem = new ItemVenta
                {
                    IdProducto = productoSeleccionado.IdProducto,
                    NombreProducto = $"{productoSeleccionado.Tipo} {productoSeleccionado.Nombre}",
                    Cantidad = cantidad,
                    PrecioUnitario = productoSeleccionado.PrecioVenta
                };
                listaItemsVenta.Add(nuevoItem);
            }

            ActualizarTotalVenta();
            LimpiarCamposProducto();
        }

        private void CargarProductos()
        {
            try
            {
                cboProductos.ItemsSource = null;
                cboProductos.Items.Clear();
                cboProductos.SelectedValuePath = "IdProducto";

                listaProductos = cnProducto.Listar();
                cboProductos.ItemsSource = listaProductos;

                if (listaProductos?.Count > 0)
                {
                    cboProductos.SelectedIndex = 0;
                }
                else
                {
                    MessageBox.Show("No se encontraron productos disponibles.", "Información",
                                   MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar productos: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (dgvItemsVenta.SelectedItem is ItemVenta itemSeleccionado)
            {
                listaItemsVenta.Remove(itemSeleccionado);
                ActualizarTotalVenta();
            }
            else
            {
                MessageBox.Show("Debe seleccionar un ítem para eliminar.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnRegistrarVenta_Click(object sender, RoutedEventArgs e)
        {
            if (listaItemsVenta.Count == 0)
            {
                MessageBox.Show("No se puede registrar una venta sin productos.", "Error",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (clienteSeleccionado == null)
            {
                clienteSeleccionado = new Cliente
                {
                    IdCliente = 0,
                    Nombre = "CONSUMIDOR",
                    Apellido = "FINAL",
                    Documento = "99999999"
                };
            }

            int metodoPagoId = ObtenerIdMetodoPagoSeleccionado();
            var metodoPagoItem = ObtenerMetodoPagoSeleccionado();

            Venta nuevaVenta = new Venta
            {
                FechaVenta = DateTime.Now,
                IdVendedor = idVendedor,
                IdCliente = clienteSeleccionado.IdCliente,
                Total = listaItemsVenta.Sum(item => item.Subtotal),
                Items = listaItemsVenta.ToList(),
                Cliente = clienteSeleccionado.NombreCompleto,
                DocumentoCliente = clienteSeleccionado.Documento,
                NombreVendedor = NombreCompleto,
                MetodoPagoId = metodoPagoId
            };

            string mensaje;
            bool ok = cnVenta.Registrar(nuevaVenta, out mensaje);

            if (ok)
            {
                MessageBox.Show("Venta registrada exitosamente.\n" + mensaje, "Éxito",
                               MessageBoxButton.OK, MessageBoxImage.Information);

                foreach (var item in listaItemsVenta)
                {
                    CN_Alerta cnAlerta = new CN_Alerta();
                    cnAlerta.VerificarAlertasProducto(item.IdProducto);
                }

                GenerarFactura(nuevaVenta);
                LimpiarFormularioCompleto();

                EventAggregator.Publish(new ProductoActualizadoEvent());
                EventAggregator.Publish(new AlertasActualizadasEvent());
            }
            else
            {
                MessageBox.Show("Error al registrar venta: " + mensaje, "Error",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerarFactura(Venta venta)
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    FlowDocument document = new FlowDocument();
                    document.PagePadding = new Thickness(50);
                    document.FontFamily = new System.Windows.Media.FontFamily("Courier New");
                    document.FontSize = 12;

                    Paragraph header = new Paragraph();
                    header.Inlines.Add(new Run("PASTAS ELVIRA\n") { FontSize = 16, FontWeight = FontWeights.Bold });
                    header.Inlines.Add(new Run("Factura de Venta\n\n"));
                    header.Inlines.Add(new Run($"Fecha: {venta.FechaVenta:dd/MM/yyyy HH:mm:ss}\n"));
                    header.Inlines.Add(new Run($"Cliente: {venta.Cliente}\n"));
                    if (!string.IsNullOrEmpty(venta.DocumentoCliente))
                        header.Inlines.Add(new Run($"Documento: {venta.DocumentoCliente}\n"));
                    header.Inlines.Add(new Run($"Vendedor: {venta.NombreVendedor}\n"));

                    var metodoPago = ObtenerMetodoPagoSeleccionado();
                    header.Inlines.Add(new Run($"Método de Pago: {ObtenerTextoMetodoPago(metodoPago)}\n"));

                    header.Inlines.Add(new Run("".PadRight(50, '=') + "\n\n"));
                    document.Blocks.Add(header);

                    Table table = new Table();
                    table.Columns.Add(new TableColumn { Width = new GridLength(200) });
                    table.Columns.Add(new TableColumn { Width = new GridLength(70) });
                    table.Columns.Add(new TableColumn { Width = new GridLength(100) });
                    table.Columns.Add(new TableColumn { Width = new GridLength(100) });

                    TableRowGroup rowGroup = new TableRowGroup();

                    TableRow headerRow = new TableRow { Background = System.Windows.Media.Brushes.LightGray };
                    headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Producto")) { FontWeight = FontWeights.Bold }));
                    headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Cant")) { FontWeight = FontWeights.Bold }));
                    headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Precio")) { FontWeight = FontWeights.Bold }));
                    headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Subtotal")) { FontWeight = FontWeights.Bold }));
                    rowGroup.Rows.Add(headerRow);

                    foreach (var item in venta.Items)
                    {
                        TableRow row = new TableRow();
                        row.Cells.Add(new TableCell(new Paragraph(new Run(item.NombreProducto))));
                        row.Cells.Add(new TableCell(new Paragraph(new Run(item.Cantidad.ToString()))));
                        row.Cells.Add(new TableCell(new Paragraph(new Run(item.PrecioUnitario.ToString("C")))));
                        row.Cells.Add(new TableCell(new Paragraph(new Run(item.Subtotal.ToString("C")))));
                        rowGroup.Rows.Add(row);
                    }

                    table.RowGroups.Add(rowGroup);
                    document.Blocks.Add(table);

                    Paragraph footer = new Paragraph();
                    footer.Inlines.Add(new Run("\n" + "".PadRight(50, '=') + "\n"));
                    footer.Inlines.Add(new Run($"TOTAL: {venta.Total.ToString("C")}") { FontSize = 14, FontWeight = FontWeights.Bold });
                    footer.Inlines.Add(new Run($"\nMÉTODO DE PAGO: {ObtenerTextoMetodoPago(metodoPago)}") { FontSize = 12, FontWeight = FontWeights.Bold });
                    footer.Inlines.Add(new Run("\n\n¡Gracias por su compra!"));
                    document.Blocks.Add(footer);

                    printDialog.PrintDocument(((IDocumentPaginatorSource)document).DocumentPaginator, "Factura Pastas Elvira");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al imprimir: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnBuscarCliente_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FormBuscarCliente formBuscar = new FormBuscarCliente();
                if (formBuscar.ShowDialog() == true)
                {
                    clienteSeleccionado = formBuscar.ClienteSeleccionado;
                    if (clienteSeleccionado != null)
                    {
                        txtCliente.Text = $"{clienteSeleccionado.NombreCompleto} (DNI: {clienteSeleccionado.Documento})";
                        txtDocumentoCliente.Text = clienteSeleccionado.Documento;
                        txtTelefonoCliente.Text = clienteSeleccionado.Telefono;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al buscar cliente: " + ex.Message, "Error",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ActualizarTotalVenta()
        {
            decimal total = listaItemsVenta.Sum(item => item.Subtotal);
            txtTotalVenta.Text = total.ToString("C");
        }

        private void LimpiarCamposProducto()
        {
            txtCantidad.Text = "1";
            cboProductos.SelectedIndex = -1;
            txtPrecioUnitario.Text = "0.00";
            txtStockDisponible.Text = "";
        }

        private void LimpiarFormularioCompleto()
        {
            listaItemsVenta.Clear();
            LimpiarCamposProducto();

            clienteSeleccionado = null;
            txtCliente.Text = "CONSUMIDOR FINAL";
            txtDocumentoCliente.Text = "";
            txtTelefonoCliente.Text = "";

            cboMetodoPago.SelectedIndex = 0;

            ActualizarTotalVenta();
            CargarInformacionVenta();
        }
    }
}