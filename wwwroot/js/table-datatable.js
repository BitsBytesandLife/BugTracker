$(function() {
	"use strict";
	
	
	    $(document).ready(function() {
			$('#example').DataTable();
		  } );
		  
		  
		  
		  $(document).ready(function() {
			var table = $('#example2').DataTable( {
				lengthChange: false,
				buttons: [ 'copy', 'excel', 'pdf', 'print']
			} );
		 
			table.buttons().container()
				.appendTo( '#example2_wrapper .col-md-6:eq(0)' );
		} );

	$(document).ready(function () {
		var table = $('#allTickets2').DataTable({
			lengthChange: false,
			buttons: ['copy', 'excel', 'pdf', 'print']
		});

		table.buttons().container()
			.appendTo('#allTickets2_wrapper .col-md-6:eq(0)');
	});


	$(document).ready(function () {
		var table = $('#myProjects').DataTable({
			lengthChange: false,
			buttons: ['copy', 'excel', 'pdf', 'print']
		});

		table.buttons().container()
			.appendTo('#allProjects_wrapper .col-md-6:eq(0)');
	});
	
	});