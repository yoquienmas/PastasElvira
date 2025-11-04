using CapaDatos;
using CapaEntidad;
using CapaNegocio;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace CapaPresentacion
{
    public partial class FormProducto : Window
    {
        private CN_Producto cnProducto = new CN_Producto();
        private Producto productoSeleccionado = null;
        private List<Producto> listaProductos;
        private List<string> listaTipos;
        private List<string> listaSabores;

        public FormProducto()
        {
            InitializeComponent();
        }

        private void VerificarErrorCargaProductos()
        {
            try
            {
                using (var connection = new SqlConnection(Conexion.cadena))
                {
                    connection.Open();

                    // Probar la consulta directamente
                    var cmd = new SqlCommand(@"
                SELECT 
                    p.IdProducto, 
                    p.Nombre,
                    t.Descripcion AS Tipo, 
                    s.Descripcion AS Sabor
                FROM Producto p
                INNER JOIN Tipo t ON p.IdTipo = t.IdTipo
                INNER JOIN Sabor s ON p.IdSabor = s.IdSabor
                WHERE p.Visible = 1", connection);

                    var reader = cmd.ExecuteReader();

                    int count = 0;
                    while (reader.Read())
                    {
                        count++;
                        Console.WriteLine($"Producto {count}: {reader["Nombre"]} - {reader["Tipo"]} - {reader["Sabor"]}");
                    }
                    reader.Close();

                    MessageBox.Show($"Consulta exitosa. Productos encontrados: {count}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error en consulta: {ex.Message}\n\n{ex.StackTrace}");
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Probar primero la consulta
            VerificarErrorCargaProductos();

            // Luego cargar los datos
            CargarTiposProducto();
            CargarSaboresProducto();
            CargarProductos();
            LimpiarFormulario();
            CalcularCostoYActualizarPrecio();
        }

        private void CargarProductos()
        {
            try
            {
                listaProductos = cnProducto.Listar();
                dgvProductos.ItemsSource = listaProductos;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar productos: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarTiposProducto()
        {
            try
            {
                listaTipos = cnProducto.ListarTiposProducto();

                // LIMPIAR Y CARGAR CORRECTAMENTE
                cboTipo.ItemsSource = null;
                cboTipo.Items.Clear();

                if (listaTipos != null && listaTipos.Count > 0)
                {
                    cboTipo.ItemsSource = listaTipos;
                    Console.WriteLine($"Tipos cargados correctamente: {listaTipos.Count}");

                    foreach (var tipo in listaTipos)
                    {
                        Console.WriteLine($" - {tipo}");
                    }
                }
                else
                {
                    Console.WriteLine("No se encontraron tipos en la base de datos");
                    MessageBox.Show("No se encontraron tipos de productos en la base de datos.",
                                  "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar tipos: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine($"Error en CargarTiposProducto: {ex.Message}");
            }
        }

        private void CargarSaboresProducto()
        {
            try
            {
                listaSabores = cnProducto.ListarSaboresProducto();

                // LIMPIAR Y CARGAR CORRECTAMENTE
                cboSabor.ItemsSource = null;
                cboSabor.Items.Clear();

                if (listaSabores != null && listaSabores.Count > 0)
                {
                    cboSabor.ItemsSource = listaSabores;
                    Console.WriteLine($"Sabores cargados correctamente: {listaSabores.Count}");

                    foreach (var sabor in listaSabores)
                    {
                        Console.WriteLine($" - {sabor}");
                    }
                }
                else
                {
                    Console.WriteLine("No se encontraron sabores en la base de datos");
                    MessageBox.Show("No se encontraron sabores de productos en la base de datos.",
                                  "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar sabores: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine($"Error en CargarSaboresProducto: {ex.Message}");
            }
        }

        private void LimpiarFormulario()
        {
            productoSeleccionado = null;
            txtStock.Text = "0";
            txtStockMinimo.Text = "10";
            txtMargenGanancia.Text = "30";
            chkVisible.IsChecked = true;

            // Reiniciar los combos PRIMERO
            cboTipo.SelectedIndex = -1;
            cboSabor.SelectedIndex = -1;

            // LUEGO limpiar el nombre (se generará automáticamente cuando selecciones combo)
            txtNombre.Text = "";
            txtNombre.ToolTip = null;

            btnAgregar.Content = "➕ Agregar";
            btnEditar.IsEnabled = false;
            btnEstado.IsEnabled = false;

            CalcularCostoYActualizarPrecio();
        }

        private void ActualizarNombreAutomatico()
        {
            // Solo para productos nuevos
            if (productoSeleccionado != null) return;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                GenerarNombreAutomatico();
            }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }

        private void txtMargenGanancia_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalcularPrecioAutomatico();
        }

        private void CalcularPrecioAutomatico()
        {
            try
            {
                if (decimal.TryParse(txtCostoProduccion.Text, out decimal costo) &&
                    decimal.TryParse(txtMargenGanancia.Text, out decimal margen))
                {
                    decimal precio = costo * (1 + (margen / 100));
                    txtPrecioVenta.Text = precio.ToString("F2");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en cálculo automático: {ex.Message}");
            }
        }

        private void CalcularCostoYActualizarPrecio()
        {
            try
            {
                decimal costosFijosUnitarios = cnProducto.CalcularCostosFijosUnitarios();
                txtCostoProduccion.Text = costosFijosUnitarios.ToString("F2");
                CalcularPrecioAutomatico();
            }
            catch (Exception ex)
            {
                txtCostoProduccion.Text = "0";
                txtPrecioVenta.Text = "0";
                System.Diagnostics.Debug.WriteLine($"Error al calcular costo: {ex.Message}");
            }
        }

        private void btnRecalcularCosto_Click(object sender, RoutedEventArgs e)
        {
            CalcularCostoYActualizarPrecio();
            MessageBox.Show("Costo recalculado correctamente", "Éxito",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnAgregar_Click(object sender, RoutedEventArgs e)
        {
            GuardarProducto(false);
        }

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            GuardarProducto(true);
        }

        private void GuardarProducto(bool esEdicion)
        {
            Console.WriteLine($"DEBUG - Nombre antes de guardar: '{txtNombre.Text}'");
            Console.WriteLine($"DEBUG - Tipo: '{cboTipo.Text}', Sabor: '{cboSabor.Text}'");

            // VALIDACIÓN EXTRA MÁS ESTRICTA
            string nombreValidado = txtNombre.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(nombreValidado))
            {
                Console.WriteLine("Nombre vacío detectado, generando automáticamente...");
                GenerarNombreAutomatico();
                nombreValidado = txtNombre.Text?.Trim() ?? "";

                // Validar nuevamente después de generar
                if (string.IsNullOrWhiteSpace(nombreValidado))
                {
                    // Último intento - forzar un nombre
                    txtNombre.Text = "Producto Sin Nombre";
                    nombreValidado = "Producto Sin Nombre";
                    Console.WriteLine("Nombre forzado a valor por defecto");
                }
            }

            // Validación FINAL definitiva
            if (string.IsNullOrWhiteSpace(nombreValidado))
            {
                MessageBox.Show("ERROR CRÍTICO: No se pudo establecer un nombre para el producto.",
                              "Error Grave", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Console.WriteLine($"DEBUG - Nombre validado: '{nombreValidado}'");

            // Resto de las validaciones...
            if (string.IsNullOrWhiteSpace(cboTipo.Text))
            {
                MessageBox.Show("El tipo es obligatorio", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(cboSabor.Text))
            {
                MessageBox.Show("El sabor es obligatorio", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Obtener IDs de Tipo y Sabor
            int idTipo = cnProducto.ObtenerIdTipoPorNombre(cboTipo.Text);
            int idSabor = cnProducto.ObtenerIdSaborPorNombre(cboSabor.Text);

            if (idTipo <= 0)
            {
                MessageBox.Show("Tipo de producto no válido", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (idSabor <= 0)
            {
                MessageBox.Show("Sabor de producto no válido", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validar combinación única
            int idProductoExcluir = esEdicion && productoSeleccionado != null ? productoSeleccionado.IdProducto : 0;
            if (cnProducto.ExisteCombinacionTipoSabor(idTipo, idSabor, idProductoExcluir))
            {
                MessageBox.Show("Ya existe un producto con esta combinación de Tipo y Sabor", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validar campos numéricos
            if (!decimal.TryParse(txtCostoProduccion.Text, out decimal costo) || costo < 0)
            {
                MessageBox.Show("El costo de producción debe ser un valor válido", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtPrecioVenta.Text, out decimal precio) || precio < 0)
            {
                MessageBox.Show("El precio de venta debe ser un valor válido", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtMargenGanancia.Text, out decimal margen) || margen < 0)
            {
                MessageBox.Show("El margen de ganancia debe ser un porcentaje válido", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtStock.Text, out int stock) || stock < 0)
            {
                MessageBox.Show("El stock actual debe ser un número válido", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtStockMinimo.Text, out int stockMinimo) || stockMinimo < 0)
            {
                MessageBox.Show("El stock mínimo debe ser un número válido", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // CREAR EL OBJETO PRODUCTO - GARANTIZAR QUE EL NOMBRE NO SEA NULL
            Producto producto = new Producto
            {
                IdTipo = idTipo,
                IdSabor = idSabor,
                Tipo = cboTipo.Text,
                Sabor = cboSabor.Text,
                Nombre = nombreValidado, // USAR LA VARIABLE VALIDADA
                CostoProduccion = costo,
                PrecioVenta = precio,
                MargenGanancia = margen,
                StockActual = stock,
                StockMinimo = stockMinimo,
                Visible = chkVisible.IsChecked ?? true
            };

            // Si es edición, asignar el ID
            if (esEdicion && productoSeleccionado != null)
            {
                producto.IdProducto = productoSeleccionado.IdProducto;
            }

            string mensaje;
            bool resultado;

            if (esEdicion)
            {
                resultado = cnProducto.Editar(producto, out mensaje);
            }
            else
            {
                resultado = cnProducto.Registrar(producto, out mensaje);
            }

            if (resultado)
            {
                MessageBox.Show(mensaje, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                CargarProductos();
                LimpiarFormulario();
            }
            else
            {
                MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
        }

        private void dgvProductos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgvProductos.SelectedItem is Producto producto)
            {
                productoSeleccionado = producto;

                // Cargar datos en los combos - BUSCAR POR DESCRIPCIÓN
                cboTipo.Text = producto.Tipo;
                cboSabor.Text = producto.Sabor;

                // Usar el nombre real de la base de datos, no generar uno nuevo
                txtNombre.Text = producto.Nombre;

                txtCostoProduccion.Text = producto.CostoProduccion.ToString("F2");
                txtPrecioVenta.Text = producto.PrecioVenta.ToString("F2");
                txtMargenGanancia.Text = producto.MargenGanancia.ToString("F2");
                txtStock.Text = producto.StockActual.ToString();
                txtStockMinimo.Text = producto.StockMinimo.ToString();
                chkVisible.IsChecked = producto.Visible;

                btnAgregar.Content = "➕ Agregar";
                btnEditar.IsEnabled = true;
                btnEstado.IsEnabled = true;

                Console.WriteLine($"Producto seleccionado: {producto.Nombre}");
                Console.WriteLine($"Tipo: {producto.Tipo}, Sabor: {producto.Sabor}");

                

            }
        }

        // ELIMINAR LOS MÉTODOS TextChanged Y USAR SOLO SelectionChanged

        private void cboTipo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Console.WriteLine("cboTipo_SelectionChanged disparado");
            ActualizarNombreAutomatico();
        }

        private void cboSabor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Console.WriteLine("cboSabor_SelectionChanged disparado");
            ActualizarNombreAutomatico();
        }

        private void cboTipo_DropDownClosed(object sender, EventArgs e)
        {
            GenerarNombreAutomatico();
        }

        private void cboSabor_DropDownClosed(object sender, EventArgs e)
        {
            GenerarNombreAutomatico();
        }

        // MÉTODO SIMPLIFICADO PARA GENERAR NOMBRE
        private void GenerarNombreAutomatico()
        {
            // Solo generar nombre automático si NO estamos editando un producto existente
            if (productoSeleccionado != null)
            {
                Console.WriteLine("Modo edición - No se genera nombre automático");
                return;
            }

            string sabor = cboSabor.SelectedItem as string ?? cboSabor.Text ?? "";
            string tipo = cboTipo.SelectedItem as string ?? cboTipo.Text ?? "";

            Console.WriteLine($"Generando nombre - Sabor: '{sabor}', Tipo: '{tipo}'");

            // GARANTIZAR QUE SIEMPRE HAYA UN NOMBRE
            if (!string.IsNullOrWhiteSpace(sabor) && !string.IsNullOrWhiteSpace(tipo))
            {
                txtNombre.Text = $"{tipo} de {sabor}";
            }
            else if (!string.IsNullOrWhiteSpace(sabor))
            {
                txtNombre.Text = sabor;
            }
            else if (!string.IsNullOrWhiteSpace(tipo))
            {
                txtNombre.Text = tipo;
            }
            else
            {
                // NUNCA dejar el nombre vacío - poner un valor por defecto
                txtNombre.Text = "Nuevo Producto";
            }

            Console.WriteLine($"Nombre generado: '{txtNombre.Text}'");
        }

        //BOTON Estado - cambia el estado del producto
        private void btnEstado_Click(object sender, RoutedEventArgs e)
        {
            if (dgvProductos.SelectedItem is not Producto producto)
            {
                MessageBox.Show("Seleccione un producto para cambiar su estado.", "Aviso",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool nuevoVisible = !producto.Visible;
            string accion = nuevoVisible ? "activar" : "desactivar";

            var confirmacion = MessageBox.Show(
                $"¿Desea {accion} el producto '{producto.Nombre}'?",
                "Confirmar cambio de visibilidad",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmacion != MessageBoxResult.Yes) return;

            string mensaje;
            bool resultado = cnProducto.CambiarVisible(producto.IdProducto, nuevoVisible, out mensaje);

            MessageBox.Show(mensaje, resultado ? "Éxito" : "Error",
                            MessageBoxButton.OK,
                            resultado ? MessageBoxImage.Information : MessageBoxImage.Error);

            if (resultado)
            {
                producto.Visible = nuevoVisible;
                chkVisible.IsChecked = producto.Visible;
                dgvProductos.Items.Refresh();
                btnEstado.Content = producto.Visible ? "🔒 Desactivar" : "✔️ Activar";
            }
        }

        // NUEVO: Botón para gestionar tipos
        private void btnGestionarTipos_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Funcionalidad para gestionar tipos de productos", "Información",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // NUEVO: Botón para gestionar sabores
        private void btnGestionarSabores_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Funcionalidad para gestionar sabores de productos", "Información",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void txtNombre_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Si el usuario edita manualmente el nombre, quitar el producto seleccionado
            if (productoSeleccionado != null && txtNombre.Text != productoSeleccionado.Nombre)
            {
                // Esto indica que el usuario está editando manualmente el nombre
                // Podemos cambiar el comportamiento si es necesario
            }

        }
    }
}