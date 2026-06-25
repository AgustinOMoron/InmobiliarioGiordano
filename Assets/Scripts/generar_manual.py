#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Genera el manual de usuario en PDF para el sistema inmobiliario."""

from fpdf import FPDF
import os, datetime

class ManualPDF(FPDF):
    def __init__(self):
        super().__init__()
        self.set_auto_page_break(auto=True, margin=20)

    def header(self):
        if self.page_no() > 1:
            self.set_font('Helvetica', 'I', 8)
            self.set_text_color(120, 120, 120)
            self.cell(0, 8, 'Inmobiliaria Giordano - Manual de Usuario', align='C')
            self.ln(5)
            self.set_draw_color(200, 200, 200)
            self.line(10, self.get_y(), 200, self.get_y())
            self.ln(3)

    def footer(self):
        if self.page_no() > 1:
            self.set_y(-15)
            self.set_font('Helvetica', 'I', 8)
            self.set_text_color(150, 150, 150)
            self.cell(0, 10, f'Página {self.page_no()}', align='C')

    def titulo_principal(self, text):
        self.set_font('Helvetica', 'B', 22)
        self.set_text_color(30, 60, 110)
        self.cell(0, 15, text, align='C')
        self.ln(10)

    def subtitulo(self, text):
        self.set_font('Helvetica', 'B', 14)
        self.set_text_color(30, 60, 110)
        self.cell(0, 10, text)
        self.ln(7)
        self.set_draw_color(30, 60, 110)
        self.line(10, self.get_y(), 200, self.get_y())
        self.ln(4)

    def seccion(self, text):
        self.set_font('Helvetica', 'B', 12)
        self.set_text_color(50, 80, 130)
        self.cell(0, 8, text)
        self.ln(6)

    def cuerpo(self, text):
        self.set_font('Helvetica', '', 10)
        self.set_text_color(40, 40, 40)
        self.multi_cell(0, 5, text)
        self.ln(2)

    def item_lista(self, text):
        self.set_font('Helvetica', '', 10)
        self.set_text_color(40, 40, 40)
        x = self.get_x()
        self.cell(6, 5, chr(8226))
        self.multi_cell(0, 5, text)
        self.ln(1)

    def campo_tabla(self, label, valor):
        self.set_font('Helvetica', 'B', 10)
        self.set_text_color(40, 40, 40)
        self.cell(50, 6, label + ':', align='R')
        self.set_font('Helvetica', '', 10)
        self.cell(0, 6, '  ' + valor)
        self.ln(5)

    def nota_importante(self, text):
        self.set_fill_color(255, 245, 220)
        self.set_draw_color(200, 160, 80)
        self.set_font('Helvetica', 'I', 9)
        self.set_text_color(120, 80, 20)
        y = self.get_y()
        self.set_x(15)
        self.multi_cell(180, 5, text, border=1, fill=True)
        self.ln(3)

    def advertencia(self, text):
        self.set_fill_color(255, 230, 230)
        self.set_draw_color(200, 80, 80)
        self.set_font('Helvetica', 'I', 9)
        self.set_text_color(140, 30, 30)
        self.set_x(15)
        self.multi_cell(180, 5, text, border=1, fill=True)
        self.ln(3)


def generar():
    pdf = ManualPDF()

    # ── PORTADA ──
    pdf.add_page()
    pdf.ln(50)
    pdf.set_font('Helvetica', 'B', 28)
    pdf.set_text_color(30, 60, 110)
    pdf.cell(0, 15, 'SISTEMA DE GESTION', align='C')
    pdf.ln(12)
    pdf.cell(0, 15, 'INMOBILIARIA', align='C')
    pdf.ln(20)
    pdf.set_font('Helvetica', '', 16)
    pdf.set_text_color(80, 80, 80)
    pdf.cell(0, 10, 'Manual de Usuario', align='C')
    pdf.ln(30)
    hoy = datetime.date.today().strftime('%d/%m/%Y')
    pdf.set_font('Helvetica', '', 11)
    pdf.cell(0, 8, f'Version 1.0  -  {hoy}', align='C')
    pdf.ln(8)
    pdf.cell(0, 8, 'Inmobiliaria Luis Alfredo Giordano', align='C')
    pdf.ln(8)
    pdf.cell(0, 8, 'Bv. Los Granaderos 2115 - 5008 - Cordoba', align='C')

    # ── INDICE ──
    pdf.add_page()
    pdf.titulo_principal('INDICE')
    pdf.ln(5)
    indice = [
        ('1', 'Introduccion'),
        ('2', 'Inicio de Sesion y Registro'),
        ('3', 'Panel Principal (Dashboard)'),
        ('4', 'Gestion de Propietarios e Inmuebles'),
        ('5', 'Gestion de Inquilinos'),
        ('6', 'Gestion de Contratos'),
        ('7', 'Gestion de Recibos'),
        ('8', 'Gestion de Servicios'),
        ('9', 'Gestion de Liquidaciones'),
        ('10', 'Generacion de PDF'),
        ('11', 'Comunicacion por WhatsApp'),
        ('12', 'Navegacion entre Pantallas'),
        ('13', 'Actualizacion Automatica de Datos'),
        ('14', 'Solucion de Problemas Comunes'),
    ]
    for num, tit in indice:
        pdf.set_font('Helvetica', '', 11)
        pdf.set_text_color(40, 40, 40)
        pdf.cell(10, 7, num + '.')
        pdf.cell(0, 7, tit)
        pdf.ln(7)

    # ── 1. INTRODUCCION ──
    pdf.add_page()
    pdf.subtitulo('1. Introduccion')
    pdf.cuerpo(
        'El Sistema de Gestion Inmobiliaria es una aplicacion desarrollada en Unity que permite '
        'administrar de forma integral los procesos de una inmobiliaria. El sistema se conecta '
        'con una base de datos en la nube (Supabase) para mantener toda la informacion sincronizada.\n\n'
        'Funcionalidades principales:\n'
        '- Gestion de propietarios e inmuebles\n'
        '- Gestion de inquilinos\n'
        '- Gestion de contratos de alquiler\n'
        '- Emision de recibos con generacion de PDF\n'
        '- Registro de servicios (impuestos, expensas, etc.)\n'
        '- Liquidacion mensual para propietarios\n'
        '- Dashboard con resumen, alertas y notificaciones\n'
        '- Envio de recordatorios por WhatsApp\n'
        '- Filtros y busqueda avanzada en todas las pantallas'
    )

    # ── 2. INICIO DE SESION ──
    pdf.add_page()
    pdf.subtitulo('2. Inicio de Sesion y Registro')
    pdf.cuerpo(
        'Al iniciar la aplicacion se muestra la pantalla de login. Si es la primera vez que usa '
        'el sistema, debe crear una cuenta de administrador.'
    )
    pdf.seccion('2.1 Crear una Cuenta')
    pdf.cuerpo(
        'Complete los siguientes campos:\n'
        '- Mail: direccion de correo electronico\n'
        '- Contrasena: minimo 6 caracteres\n'
        '- Nombre: su nombre completo\n\n'
        'Presione el boton "Crear Cuenta". Recibira un correo de confirmacion. '
        'Una vez confirmada la cuenta, ya puede iniciar sesion.'
    )
    pdf.nota_importante(
        'IMPORTANTE: La contrasena debe tener al menos 6 caracteres. '
        'Revise su bandeja de entrada y spam para confirmar la cuenta.'
    )
    pdf.seccion('2.2 Iniciar Sesion')
    pdf.cuerpo(
        'Ingrese su mail y contrasena registrados, luego presione "Iniciar Sesion". '
        'Si los datos son correctos, accedera al panel principal.'
    )
    pdf.advertencia(
        'Si olvido su contrasena, debe utilizar la opcion de recuperacion de Supabase '
        'desde la pagina de login del sistema.'
    )

    # ── 3. PANEL PRINCIPAL ──
    pdf.add_page()
    pdf.subtitulo('3. Panel Principal (Dashboard)')
    pdf.cuerpo(
        'El panel principal es la pantalla central del sistema. Se organiza en tres secciones '
        'desplazables (ScrollViews) y un sistema de notificaciones.'
    )
    pdf.seccion('3.1 Aumentos por Aplicar')
    pdf.cuerpo(
        'Muestra los contratos que estan proximos a cumplir el periodo de ajuste por IPC. '
        'Cada fila muestra: numero de contrato, nombre del inquilino, mes del aumento, '
        'importe actual, nuevo importe (con incremento del 10%) y la variacion porcentual.\n\n'
        'Cada item tiene un boton de WhatsApp para enviar un aviso de aumento al inquilino.'
    )
    pdf.seccion('3.2 Proximos Contratos a Vencer')
    pdf.cuerpo(
        'Lista los contratos cuya fecha de finalizacion esta dentro de los proximos 2 meses. '
        'Se muestra: nombre del inquilino, direccion del inmueble, numero de contrato, '
        'fecha de vencimiento y dias restantes.\n\n'
        'Los contratos con menos de 15 dias se muestran en color rojo. '
        'Cada item tiene un boton de WhatsApp para avisar al inquilino.'
    )
    pdf.seccion('3.3 Tabla Resumen')
    pdf.cuerpo(
        'Tabla completa de todos los contratos activos, agrupada alfabeticamente por apellido '
        'del propietario. Muestra: propietario, inquilino, direccion, monto de alquiler, '
        'servicios/impuestos, comisiones y fecha de finalizacion.\n\n'
        'Incluye un buscador en tiempo real que filtra por nombre de propietario, inquilino, '
        'direccion o numero de contrato (formato: CT-AAAA-####).'
    )
    pdf.seccion('3.4 Sistema de Notificaciones')
    pdf.cuerpo(
        'El icono de campana en la esquina superior muestra un badge con la cantidad de '
        'notificaciones activas (maximo "9+").\n\n'
        'Al hacer clic en la campana se despliega un panel con las notificaciones. '
        'Existen dos tipos:\n'
        '- Alertas de aumento (1 mes de anticipacion)\n'
        '- Alertas de vencimiento (2 meses de anticipacion)\n\n'
        'Cada notificacion puede eliminarse individualmente o puede usar el boton '
        '"Borrar Todas". Las notificaciones eliminadas se recuerdan incluso al cerrar '
        'la aplicacion.'
    )

    # ── 4. PROPIETARIOS E INMUEBLES ──
    pdf.add_page()
    pdf.subtitulo('4. Gestion de Propietarios e Inmuebles')
    pdf.cuerpo(
        'Esta pantalla unificada permite administrar propietarios y sus inmuebles en un solo lugar. '
        'Del lado izquierdo se muestra la lista de propietarios; del lado derecho, el formulario '
        'y la seccion de inmuebles del propietario seleccionado.'
    )
    pdf.seccion('4.1 Propietarios')
    pdf.cuerpo(
        'Campos del formulario:\n'
        '- Nombre: obligatorio\n'
        '- Apellido: obligatorio\n'
        '- Telefono: numero valido obligatorio\n\n'
        'Acciones:\n'
        '- Buscar propietarios por texto\n'
        '- Ver todos los propietarios\n'
        '- Agregar nuevo propietario\n'
        '- Editar propietario existente\n'
        '- Eliminar propietario (debe eliminar sus inmuebles primero)'
    )
    pdf.seccion('4.2 Inmuebles')
    pdf.cuerpo(
        'Al seleccionar o crear un propietario, se habilita la seccion de inmuebles. '
        'Cada inmueble puede ser de tipo: Casa, Departamento o Salon Comercial.\n\n'
        'Campos del formulario modal:\n'
        '- Direccion: obligatorio\n'
        '- Numero: obligatorio\n'
        '- Tipo: Casa / Departamento / Salon Comercial\n'
        '- Piso y Unidad: solo para Departamentos\n\n'
        'Al seleccionar "Departamento" se habilitan los campos de piso y unidad.'
    )
    pdf.nota_importante(
        'CONSEJO: Primero registre el propietario, luego agregue sus inmuebles. '
        'Al guardar un propietario nuevo, el sistema lo selecciona automaticamente '
        'para que pueda agregar inmuebles de inmediato.'
    )

    # ── 5. INQUILINOS ──
    pdf.add_page()
    pdf.subtitulo('5. Gestion de Inquilinos')
    pdf.cuerpo(
        'Pantalla de administracion de inquilinos con busqueda incluida.'
    )
    pdf.seccion('5.1 Formulario')
    pdf.cuerpo(
        'Campos:\n'
        '- Nombre: obligatorio\n'
        '- Apellido: obligatorio\n'
        '- Telefono: numero valido obligatorio'
    )
    pdf.seccion('5.2 Acciones')
    pdf.cuerpo(
        '- Buscar inquilinos por texto (Enter para buscar)\n'
        '- Mostrar todos los inquilinos\n'
        '- Agregar nuevo inquilino\n'
        '- Editar inquilino\n'
        '- Eliminar inquilino (con confirmacion)'
    )

    # ── 6. CONTRATOS ──
    pdf.add_page()
    pdf.subtitulo('6. Gestion de Contratos')
    pdf.cuerpo(
        'Pantalla principal para la gestion de contratos de alquiler. Vincula propietarios, '
        'inquilinos e inmuebles en un mismo registro.'
    )
    pdf.seccion('6.1 Filtros y Lista')
    pdf.cuerpo(
        'En la parte izquierda puede:\n'
        '- Ver todos los contratos\n'
        '- Ver solo contratos activos\n\n'
        'La lista se ordena alfabeticamente por apellido del inquilino. '
        'Cada item muestra la direccion del inmueble, nombre del inquilino, '
        'fechas del contrato, monto y estado (Activo/Inactivo).'
    )
    pdf.seccion('6.2 Formulario')
    pdf.cuerpo(
        'Campos:\n'
        '- Fecha de Inicio: obligatorio (formato DD/MM/AAAA)\n'
        '- Fecha de Fin: opcional (formato DD/MM/AAAA)\n'
        '- Monto de Alquiler: obligatorio, numero mayor a cero\n'
        '- Honorario: porcentaje (ej: 10 = 10%)\n'
        '- Indice IPC: periodo de ajuste en meses\n'
        '- Estado: toggle (Activado = Activo, Desactivado = Inactivo)\n'
        '- Propietario: seleccion de lista desplegable\n'
        '- Inquilino: seleccion de lista desplegable\n'
        '- Inmueble: se filtra automaticamente segun el propietario seleccionado'
    )
    pdf.seccion('6.3 Acciones desde la Lista')
    pdf.cuerpo(
        'Cada contrato en la lista tiene:\n'
        '- Editar: carga los datos en el formulario\n'
        '- Eliminar: con confirmacion previa\n'
        '- WhatsApp: envia recordatorio de pago al inquilino'
    )
    pdf.nota_importante(
        'CONSEJO: Al seleccionar un propietario en el dropdown, el dropdown de inmuebles '
        'se actualiza automaticamente para mostrar solo las propiedades de ese dueno.'
    )

    # ── 7. RECIBOS ──
    pdf.add_page()
    pdf.subtitulo('7. Gestion de Recibos')
    pdf.cuerpo(
        'Emision y gestion de recibos de alquiler con generacion de PDF profesional.'
    )
    pdf.seccion('7.1 Filtros')
    pdf.cuerpo(
        'Puede filtrar recibos por periodo ingresando fecha desde y fecha hasta '
        '(formato DD/MM/AAAA). Use "Ver Todos" para mostrar todos los recibos.'
    )
    pdf.seccion('7.2 Formulario')
    pdf.cuerpo(
        'Campos:\n'
        '- Fecha: obligatorio (DD/MM/AAAA)\n'
        '- Monto: obligatorio\n'
        '- Total a Abonar: se auto-completa con el monto\n'
        '- Tipo: Alquiler / Expensas / Otros\n'
        '- Contrato: seleccion de lista desplegable\n\n'
        'Al seleccionar un contrato, el monto se auto-completa con el valor del alquiler.'
    )
    pdf.seccion('7.3 PDF')
    pdf.cuerpo(
        'Cada recibo en la lista tiene un boton "PDF" que genera un documento profesional '
        'de 2 paginas: ORIGINAL (para el inquilino) y DUPLICADO (para la inmobiliaria).\n\n'
        'El PDF incluye:\n'
        '- Datos de la inmobiliaria (nombre, direccion, telefono, email, CUIT)\n'
        '- Logo de la empresa\n'
        '- Numero de recibo y fecha\n'
        '- Nombre del inquilino y direccion del inmueble\n'
        '- Monto en pesos y concepto\n'
        '- Linea de firma\n\n'
        'El PDF se abre automaticamente al generarse.'
    )

    # ── 8. SERVICIOS ──
    pdf.add_page()
    pdf.subtitulo('8. Gestion de Servicios')
    pdf.cuerpo(
        'Registro de servicios vinculados a propietarios y propiedades, como impuestos '
        'municipales, agua, luz, gas, etc.'
    )
    pdf.seccion('8.1 Filtros')
    pdf.cuerpo(
        'Puede filtrar los servicios por propietario usando el dropdown de filtro. '
        'La opcion "Todos los Propietarios" muestra todos los servicios.'
    )
    pdf.seccion('8.2 Formulario')
    pdf.cuerpo(
        'Campos:\n'
        '- Nombre del Servicio: obligatorio (ej: MUNICIPAL, AGUA, LUZ)\n'
        '- Monto: obligatorio, numero decimal\n'
        '- Fecha: obligatorio (DD/MM/AAAA)\n'
        '- Porcentaje: 0 a 100, representa la parte que paga el propietario\n'
        '- Propietario: seleccion de lista desplegable\n'
        '- Propiedad: opcional, se filtra segun el propietario seleccionado'
    )
    pdf.seccion('8.3 Vista en Dashboard')
    pdf.cuerpo(
        'Los servicios registrados aparecen en la tabla resumen del panel principal, '
        'mostrando el nombre y monto correspondiente a cada propiedad.'
    )

    # ── 9. LIQUIDACIONES ──
    pdf.add_page()
    pdf.subtitulo('9. Gestion de Liquidaciones')
    pdf.cuerpo(
        'Liquidacion mensual para propietarios con calculo automatico del neto a pagar.'
    )
    pdf.seccion('9.1 Filtros')
    pdf.cuerpo(
        'Filtre las liquidaciones por periodo (fecha desde - fecha hasta). '
        'Use "Mostrar Todas" para limpiar el filtro.'
    )
    pdf.seccion('9.2 Formulario')
    pdf.cuerpo(
        'Campos:\n'
        '- Contrato: seleccion de contratos activos\n'
        '- Fecha: obligatorio (DD/MM/AAAA)\n'
        '- Monto de Alquiler: se auto-completa al seleccionar contrato\n'
        '- Honorarios: se calculan automaticamente (Monto x % Honorario / 100)\n'
        '- Descuento Adicional: opcional\n'
        '- Descripcion del Descuento: opcional\n'
        '- Neto Propietario: calculado automaticamente (Monto - Honorarios - Descuento)\n'
        '- Estado: Pendiente / Pagada'
    )
    pdf.seccion('9.3 Calculo Automatico')
    pdf.cuerpo(
        'El sistema calcula en tiempo real:\n'
        '- Honorarios = Monto de Alquiler x Porcentaje de Honorario del contrato / 100\n'
        '- Neto = Monto de Alquiler - Honorarios - Descuento Adicional\n\n'
        'Si el resultado es negativo, se muestra $0.'
    )
    pdf.seccion('9.4 PDF de Liquidacion')
    pdf.cuerpo(
        'Cada liquidacion tiene un boton "PDF" que genera un documento con los datos del '
        'propietario, inmueble, detalle de montos y el neto a pagar.'
    )

    # ── 10. PDF ──
    pdf.add_page()
    pdf.subtitulo('10. Generacion de PDF')
    pdf.cuerpo(
        'El sistema genera dos tipos de documentos PDF utilizando la libreria QuestPDF:'
    )
    pdf.seccion('10.1 Recibo (2 paginas)')
    pdf.cuerpo(
        'Pagina 1 - ORIGINAL: para entregar al inquilino\n'
        'Pagina 2 - DUPLICADO: para archivo de la inmobiliaria\n\n'
        'Contenido:\n'
        '- Encabezado con logo y datos de la inmobiliaria\n'
        '- Numero de recibo y fecha\n'
        '- Datos del inquilino y propiedad\n'
        '- Monto y tipo de recibo\n'
        '- Firma\n\n'
        'Los PDFs se guardan en: {carpeta_datos}/RecibosPDF/'
    )
    pdf.seccion('10.2 Liquidacion (2 paginas)')
    pdf.cuerpo(
        'Misma estructura que el recibo pero con datos de liquidacion:\n'
        '- Propietario, inmueble y numero de contrato\n'
        '- Detalle: monto de alquiler, honorarios, descuentos\n'
        '- Neto a pagar al propietario\n\n'
        'Los PDFs se guardan en: {carpeta_datos}/LiquidacionesPDF/'
    )
    pdf.nota_importante(
        'Los PDF se abren automaticamente al generarse. Si el sistema no puede abrirlos, '
        'puede encontrarlos en la carpeta de datos de la aplicacion.'
    )

    # ── 11. WHATSAPP ──
    pdf.add_page()
    pdf.subtitulo('11. Comunicacion por WhatsApp')
    pdf.cuerpo(
        'El sistema integra 4 plantillas de mensajes predefinidos para comunicarse '
        'con los inquilinos a traves de WhatsApp.'
    )
    pdf.seccion('11.1 Recordatorio de Pago')
    pdf.cuerpo(
        'Disponible en la lista de contratos. Envia un mensaje recordando el pago del '
        'alquiler del mes en curso, incluyendo la direccion de la propiedad y el monto.'
    )
    pdf.seccion('11.2 Aviso de Aumento')
    pdf.cuerpo(
        'Disponible en la seccion "Aumentos por Aplicar" del panel principal. '
        'Informa al inquilino sobre el ajuste del alquiler segun el contrato.'
    )
    pdf.seccion('11.3 Aviso de Vencimiento')
    pdf.cuerpo(
        'Disponible en la seccion "Proximos Contratos a Vencer" del panel principal. '
        'Notifica al inquilino que su contrato esta proximo a vencer.'
    )
    pdf.nota_importante(
        'Los mensajes de WhatsApp se abren en el navegador web (web.whatsapp.com) '
        'o en la aplicacion de escritorio de WhatsApp. Asegurese de tener WhatsApp '
        'disponible en su dispositivo.'
    )

    # ── 12. NAVEGACION ──
    pdf.add_page()
    pdf.subtitulo('12. Navegacion entre Pantallas')
    pdf.cuerpo(
        'El sistema organiza las pantallas mediante un menu de navegacion. '
        'Cada boton del menu muestra la pantalla correspondiente y oculta las anteriores.'
    )
    pdf.seccion('12.1 Pantallas Disponibles')
    pdf.cuerpo(
        '1. Panel Principal (Dashboard)\n'
        '2. Propietarios e Inmuebles\n'
        '3. Inquilinos\n'
        '4. Contratos\n'
        '5. Recibos\n'
        '6. Servicios\n'
        '7. Liquidaciones'
    )
    pdf.seccion('12.2 Comportamiento')
    pdf.cuerpo(
        'Al cambiar de pantalla, el sistema:\n'
        '- Oculta la pantalla actual\n'
        '- Muestra la pantalla seleccionada\n'
        '- Carga los datos mas recientes desde la base de datos\n'
        '- Cierra el menu lateral si estaba abierto'
    )
    pdf.nota_importante(
        'Los datos se cargan automaticamente cada vez que ingresa a una pantalla. '
        'Si realizo cambios en otra seccion, apareceran reflejados al volver.'
    )

    # ── 13. ACTUALIZACION ──
    pdf.add_page()
    pdf.subtitulo('13. Actualizacion Automatica de Datos')
    pdf.cuerpo(
        'El sistema cuenta con un sistema de eventos globales que mantiene todas las pantallas '
        'sincronizadas automaticamente.'
    )
    pdf.seccion('13.1 Funcionamiento')
    pdf.cuerpo(
        'Cuando se realiza una operacion de alta, modificacion o baja en cualquier pantalla, '
        'el sistema notifica automaticamente a las demas pantallas para que actualicen sus datos.\n\n'
        'Por ejemplo:\n'
        '- Si agrega un nuevo propietario desde la pantalla de Propietarios, al volver a '
        'Contratos el dropdown de propietarios ya incluira el nuevo registro.\n'
        '- Si modifica un servicio, el dashboard se actualiza instantaneamente.\n'
        '- Si elimina un inquilino, el dropdown de inquilinos en Contratos se actualiza solo.'
    )
    pdf.seccion('13.2 Pantallas Afectadas')
    pdf.cuerpo(
        'Los dropdowns de las siguientes pantallas se actualizan automaticamente:\n'
        '- Contratos: dropdowns de propietario, inquilino e inmueble\n'
        '- Recibos: dropdown de contratos\n'
        '- Servicios: dropdowns de propietarios y propiedades\n'
        '- Liquidaciones: dropdown de contratos\n'
        '- Dashboard: tabla resumen, aumentos, vencimientos y notificaciones'
    )

    # ── 14. SOLUCION DE PROBLEMAS ──
    pdf.add_page()
    pdf.subtitulo('14. Solucion de Problemas Comunes')
    problemas = [
        ('No puedo iniciar sesion',
         'Verifique que el mail y la contrasena sean correctos. Asegurese de haber '
         'confirmado su cuenta a traves del correo electronico.'),
        ('Los dropdowns estan vacios',
         'Verifique que haya datos cargados en las pantallas correspondientes '
         '(propietarios, inquilinos, etc.). Si el problema persiste, cierre y vuelva '
         'a abrir la pantalla.'),
        ('No se genera el PDF',
         'Asegurese de tener permisos de escritura en la carpeta de la aplicacion. '
         'Puede encontrar los PDFs en la carpeta de datos persistentes.'),
        ('No se abre WhatsApp',
         'Verifique que tenga una sesion de WhatsApp Web abierta en su navegador '
         'o la aplicacion de escritorio instalada.'),
        ('Los datos no se guardan',
         'Verifique su conexion a internet. El sistema requiere conexion con Supabase '
         '(base de datos en la nube). Revise que los campos obligatorios esten completos.'),
        ('El formulario no responde',
         'Revise que todos los campos obligatorios esten completos. Si el boton de guardar '
         'esta deshabilitado, espere a que termine la operacion anterior.'),
        ('Las notificaciones no aparecen',
         'Las notificaciones se generan automaticamente al cargar el dashboard. '
         'Verifique que haya contratos con aumentos proximos o vencimientos cercanos.'),
    ]
    for titulo, desc in problemas:
        pdf.seccion(titulo)
        pdf.cuerpo(desc)

    # ── Guardar PDF ──
    output_path = os.path.join(os.path.dirname(__file__), 'Manual de Usuario - Inmobiliaria.pdf')
    pdf.output(output_path)
    print(f'PDF generado: {output_path}')
    return output_path


if __name__ == '__main__':
    generar()
