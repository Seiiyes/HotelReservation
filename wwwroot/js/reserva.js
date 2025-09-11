$(document).ready(function() {
    // Configuración de validación
    $.validator.setDefaults({
        errorElement: 'span',
        errorClass: 'text-danger',
        highlight: function (element, errorClass, validClass) {
            $(element).addClass('is-invalid');
        },
        unhighlight: function (element, errorClass, validClass) {
            $(element).removeClass('is-invalid');
        }
    });

    // Validación personalizada para fecha mínima
    $.validator.addMethod("minDate", function(value, element) {
        if (!value) return true;
        var inputDate = new Date(value);
        var today = new Date();
        today.setHours(0,0,0,0);
        return inputDate >= today;
    }, "La fecha no puede ser anterior a hoy");

    // Validación personalizada para fecha de salida
    $.validator.addMethod("checkOutDate", function(value, element) {
        if (!value) return true;
        var checkOut = new Date(value);
        var checkIn = new Date($("#FechaEntrada").val());
        return checkOut > checkIn;
    }, "La fecha de salida debe ser posterior a la fecha de entrada");

    // Validación del formulario
    $("#reservaForm").validate({
        rules: {
            ClienteId: {
                required: true
            },
            HabitacionId: {
                required: true
            },
            FechaEntrada: {
                required: true,
                minDate: true
            },
            FechaSalida: {
                required: true,
                checkOutDate: true
            }
        },
        messages: {
            ClienteId: {
                required: "Por favor, seleccione un cliente"
            },
            HabitacionId: {
                required: "Por favor, seleccione una habitación"
            },
            FechaEntrada: {
                required: "La fecha de entrada es obligatoria"
            },
            FechaSalida: {
                required: "La fecha de salida es obligatoria"
            }
        }
    });

    // Manejo de cambios en las fechas
    $("#FechaEntrada").change(function() {
        var fechaEntrada = $(this).val();
        var fechaSalida = $("#FechaSalida");
        
        // Actualiza la fecha mínima de salida
        fechaSalida.attr("min", fechaEntrada);
        
        // Si la fecha de salida es menor o igual a la de entrada, la actualiza
        if (new Date(fechaSalida.val()) <= new Date(fechaEntrada)) {
            var nuevaFechaSalida = new Date(fechaEntrada);
            nuevaFechaSalida.setDate(nuevaFechaSalida.getDate() + 1);
            fechaSalida.val(nuevaFechaSalida.toISOString().split('T')[0]);
        }
    });

    // Formateo de fechas al cargar
    var today = new Date().toISOString().split('T')[0];
    var tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    tomorrow = tomorrow.toISOString().split('T')[0];
    
    $("#FechaEntrada").attr("min", today);
    $("#FechaSalida").attr("min", tomorrow);

    if (!$("#FechaEntrada").val()) {
        $("#FechaEntrada").val(today);
    }
    if (!$("#FechaSalida").val()) {
        $("#FechaSalida").val(tomorrow);
    }
});
