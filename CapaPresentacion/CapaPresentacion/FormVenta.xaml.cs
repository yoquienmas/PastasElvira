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
            private List<Producto> listaProductos;
            private List<Cliente> listaClientes;
            private ObservableCollection<ItemVenta> listaItemsVenta = new ObservableCollection<ItemVenta>();
            private int idVendedor;
            private string NombreCompleto;

        public FormVenta(int idUsuario, string NombreVendedor)
        {
            InitializeComponent();
            idVendedor = idUsuario;
            NombreCompleto = NombreVendedor;

            // DEBUG: Ver qué está llegando
            MessageBox.Show($"ID: {idUsuario}, Nombre recibido: '{NombreVendedor}'",
                           "Debug - FormVenta Constructor");

            // Asignar inmediatamente
            txtVendedor.Text = NombreCompleto;

            listaItemsVenta = new ObservableCollection<ItemVenta>();
            dgvItemsVenta.ItemsSource = listaItemsVenta;
            listaProductos = new List<Producto>();
            listaClientes = new List<Cliente>();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                CargarProductos();
                CargarClientes();
                CargarInformacionVenta();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar formulario: {ex.Message}", "Error",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarClientes()
        {
            try
            {
                // Limpiar el ComboBox antes de asignar nuevos items
                cboClientes.ItemsSource = null;
                cboClientes.Items.Clear();

                // Primero establecer las propiedades del ComboBox
                cboClientes.DisplayMemberPath = "NombreCompleto";
                cboClientes.SelectedValuePath = "IdCliente";

                // Luego cargar los datos
                listaClientes = cnCliente.ListarClientes();
                cboClientes.ItemsSource = listaClientes;

                // DEBUG: Verificar que se cargaron clientes
                MessageBox.Show($"Clientes cargados: {listaClientes?.Count ?? 0}");

                // Seleccionar "Consumidor Final" por defecto o crear uno
                var consumidorFinal = listaClientes?.FirstOrDefault(c => c.Nombre == "CONSUMIDOR FINAL");
                if (consumidorFinal != null)
                {
                    cboClientes.SelectedItem = consumidorFinal;
                }
                else if (listaClientes?.Count > 0)
                {
                    cboClientes.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar clientes: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarInformacionVenta()
        {
            // Información del vendedor - asignar directamente
            txtVendedor.Text = NombreCompleto;

            // DEBUG
            Console.WriteLine($"Asignando vendedor: {NombreCompleto}");

            // Fecha actual
            txtFecha.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        }

        private void cboClientes_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (cboClientes.SelectedItem is Cliente clienteSeleccionado)
                {
                    // Llenar automáticamente los campos del cliente
                    txtDocumentoCliente.Text = clienteSeleccionado.Documento;
                    txtTelefonoCliente.Text = clienteSeleccionado.Telefono;
                }
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

                // Verificar stock disponible
                if (productoSeleccionado.StockActual < cantidad)
                {
                    MessageBox.Show($"Stock insuficiente. Solo hay {productoSeleccionado.StockActual} unidades disponibles.",
                                    "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Verificar si el producto ya está en la lista
                var itemExistente = listaItemsVenta.FirstOrDefault(item => item.IdProducto == productoSeleccionado.IdProducto);
                if (itemExistente != null)
                {
                    itemExistente.Cantidad += cantidad;
                }
                else
                {
                    ItemVenta nuevoItem = new ItemVenta
                    {
                        IdProducto = productoSeleccionado.IdProducto,
                        NombreProducto = productoSeleccionado.NombreProducto,
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
                // Limpiar el ComboBox antes de asignar nuevos items
                cboProductos.ItemsSource = null;
                cboProductos.Items.Clear();

                // Establecer las propiedades del ComboBox
                cboProductos.DisplayMemberPath = "NombreProducto";
                cboProductos.SelectedValuePath = "IdProducto";

                // Luego cargar los datos
                listaProductos = cnProducto.Listar();
                cboProductos.ItemsSource = listaProductos;

                // DEBUG: Verificar que se cargaron productos
                MessageBox.Show($"Productos cargados: {listaProductos?.Count ?? 0}");

                // Verificar los datos específicos
                if (listaProductos?.Count > 0)
                {
                    foreach (var producto in listaProductos)
                    {
                        Console.WriteLine($"Producto: {producto.Nombre}, Tipo: {producto.Tipo}, NombreProducto: {producto.NombreProducto}");
                    }
                    cboProductos.SelectedIndex = 0;
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
                MessageBox.Show("No se puede registrar una venta sin productos.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Obtener el cliente seleccionado
            if (cboClientes.SelectedItem == null)
            {
                MessageBox.Show("Debe seleccionar un cliente.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Cliente clienteSeleccionado = (Cliente)cboClientes.SelectedItem;

            Venta nuevaVenta = new Venta
            {
                FechaVenta = DateTime.Now,
                IdVendedor = idVendedor,
                IdCliente = clienteSeleccionado.IdCliente,
                Total = listaItemsVenta.Sum(item => item.Subtotal),
                Items = listaItemsVenta.ToList(),

                // ✅ AGREGAR ESTAS PROPIEDADES
                Cliente = clienteSeleccionado.NombreCompleto,
                DocumentoCliente = clienteSeleccionado.Documento,
                NombreVendedor = NombreCompleto
            };

            string mensaje;
            bool ok = cnVenta.Registrar(nuevaVenta, out mensaje);

            if (ok)
            {
                MessageBox.Show("Venta registrada exitosamente.\n" + mensaje, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                GenerarFactura(nuevaVenta);
                LimpiarFormularioCompleto();
            }
            else
            {
                MessageBox.Show("Error al registrar venta: " + mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnImprimir_Click(object sender, RoutedEventArgs e)
            {
                if (listaItemsVenta.Count == 0)
                {
                    MessageBox.Show("No hay productos para imprimir.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Obtener cliente seleccionado o usar "CONSUMIDOR FINAL"
                string nombreCliente = "CONSUMIDOR FINAL";
                string documentoCliente = "";

                if (cboClientes.SelectedItem is Cliente clienteSeleccionado)
                {
                    nombreCliente = clienteSeleccionado.NombreCompleto;
                    documentoCliente = clienteSeleccionado.Documento;
                }

                Venta ventaTemporal = new Venta
                {
                    FechaVenta = DateTime.Now,
                    IdVendedor = idVendedor,
                    NombreVendedor = NombreCompleto,
                    Cliente = nombreCliente,
                    DocumentoCliente = documentoCliente,
                    Items = listaItemsVenta.ToList(),
                    Total = listaItemsVenta.Sum(item => item.Subtotal)
                };

                GenerarFactura(ventaTemporal);
            }

            private void GenerarFactura(Venta venta)
            {
                try
                {
                    PrintDialog printDialog = new PrintDialog();
                    if (printDialog.ShowDialog() == true)
                    {
                        // Crear documento para impresión
                        FlowDocument document = new FlowDocument();
                        document.PagePadding = new Thickness(50);
                        document.FontFamily = new System.Windows.Media.FontFamily("Courier New");
                        document.FontSize = 12;

                        // Encabezado
                        Paragraph header = new Paragraph();
                        header.Inlines.Add(new Run("PASTAS ELVIRA\n") { FontSize = 16, FontWeight = FontWeights.Bold });
                        header.Inlines.Add(new Run("Factura de Venta\n\n"));
                        header.Inlines.Add(new Run($"Fecha: {venta.FechaVenta:dd/MM/yyyy HH:mm:ss}\n"));
                        header.Inlines.Add(new Run($"Cliente: {venta.Cliente}\n"));
                        if (!string.IsNullOrEmpty(venta.DocumentoCliente))
                            header.Inlines.Add(new Run($"Documento: {venta.DocumentoCliente}\n"));
                        header.Inlines.Add(new Run($"Vendedor: {venta.NombreVendedor}\n"));
                        header.Inlines.Add(new Run("".PadRight(50, '=') + "\n\n"));
                        document.Blocks.Add(header);

                        // Detalle de productos
                        Table table = new Table();
                        table.Columns.Add(new TableColumn { Width = new GridLength(300) }); // Producto
                        table.Columns.Add(new TableColumn { Width = new GridLength(80) });  // Cantidad
                        table.Columns.Add(new TableColumn { Width = new GridLength(100) }); // Precio
                        table.Columns.Add(new TableColumn { Width = new GridLength(100) }); // Subtotal

                        TableRowGroup rowGroup = new TableRowGroup();

                        // Encabezado de tabla
                        TableRow headerRow = new TableRow { Background = System.Windows.Media.Brushes.LightGray };
                        headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Producto")) { FontWeight = FontWeights.Bold }));
                        headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Cant")) { FontWeight = FontWeights.Bold }));
                        headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Precio")) { FontWeight = FontWeights.Bold }));
                        headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Subtotal")) { FontWeight = FontWeights.Bold }));
                        rowGroup.Rows.Add(headerRow);

                        // Productos
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

                        // Total
                        Paragraph footer = new Paragraph();
                        footer.Inlines.Add(new Run("\n" + "".PadRight(50, '=') + "\n"));
                        footer.Inlines.Add(new Run($"TOTAL: {venta.Total.ToString("C")}") { FontSize = 14, FontWeight = FontWeights.Bold });
                        footer.Inlines.Add(new Run("\n\n¡Gracias por su compra!"));
                        document.Blocks.Add(footer);

                        // Imprimir
                        printDialog.PrintDocument(((IDocumentPaginatorSource)document).DocumentPaginator, "Factura Pastas Elvira");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al imprimir: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

                // Restablecer selección de cliente
                if (cboClientes.Items.Count > 0)
                {
                    var consumidorFinal = listaClientes.FirstOrDefault(c => c.Nombre == "CONSUMIDOR FINAL");
                    if (consumidorFinal != null)
                    {
                        cboClientes.SelectedItem = consumidorFinal;
                    }
                    else
                    {
                        cboClientes.SelectedIndex = 0;
                    }
                }

                ActualizarTotalVenta();
                CargarInformacionVenta();
            }
    } 
}