﻿using BAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAL.Common
{
    public class EmailTemplates
    {
        public const string ORDER_TEMPLATE = @"<html>
                                                    <body>
                                                    <h1 align='center'>Thank you for your order</h1>
                                                    <h2 align='center'>Please find your order details below</h2>
                                                    <h2 align='center'>Order Number : {{OrderId}}</h2>
                                                    <table border='2' align='center' width='80%'>
                                                    <tr>
                                                    <td> S.No </td>
                                                    <td> Product </td>
                                                    <td> Product Name </td>
                                                    <td> Price </td>
                                                    <td> Quantity </td>
                                                    <td> Total Price </td>
                                                    </tr>
                                                    {{OrderDetailsHTML}}
                                                    </table>
                                                    </body>
                                                    </html>";


        public const string CUSTOMER_TEMPLATE = @"<html>
                                                     <body style='font-family:Calibri'>
                                                      <h1 align='center'>Thank you for registering with us</h1>
                                                      <h2 align='center'> please chek you registration details</h2>
                                                      <h2 align= 'center'>Registraion ID :{{CustomerId}}
                                                      <br /><br />
                                                      <table border='0'>
                                                      <tr>
                                                      <td> <b>User Id</b> </td>
                                                      <td> {{CUST_EMAIL}} </td>
                                                      </tr>
                                                      <tr>
                                                      <td> <b>Full Name</b> </td>
                                                      <td> {{CUST_FULL_NAME}} </td>                                                      
                                                      </tr>
                                                      <tr>
                                                      <td colspan='4'>
                                                      <br /><br /><br /><br /><br /><br /><br /><br /><br />
                                                      <img src='http://ec2-34-224-189-196.compute-1.amazonaws.com:5173/assets/logo2-BRJOyuYn.png' width='200px' height='45px' /> <br />
                                                      <h3>Team - PharmETrade</h3>
                                                      </td>
                                                      </tr>
                                                      </table>
                                                      </body>
                                                      </html>";

        public const string CUSTOMER_INVOICE = @"<html>
                                                    <body style='font-family:Calibri'>
                                                    <table border='1' align='center' width='80%'>
                                                    <tr>
                                                    <td>
                                                    <table border='0' align='center' width='95%'>
                                                    <tr>
                                                    <td align='left' margin='10'> <h1> INVOICE </h1>  </td>
                                                    <td align='right'>  
	                                                    <h3> PharmETrade </h3>
	                                                    <h5> 36 Roremond </h5>
	                                                    <h5> WAYNE, USA </h5>
	                                                    <h5> 99887 </h5>
	                                                    </td>
                                                    </tr>
                                                    <tr>
                                                    <td colspan='2'>
                                                    <hr />
                                                    </td>
                                                    </tr>
                                                    <tr> 
	                                                    <td> 
		                                                    <b> 
			                                                    <u>BILL TO :</u> <br />
                                                            </b> 
			                                                    <span>{{CUST_NAME}}</span> <br />
			                                                    <span>{{CUST_ADDRESS1}}</span> <br />
			                                                    <span>{{CUST_ADDRESS2}}</span> <br />
			                                                    <span>{{CUST_COUNTRY}}</span> <br />
			                                                    <span>{{CUST_PINCODE}}</span> <br />
	                                                    </td> 
	                                                    <td align='right'> 
		                                                    <b> 
			                                                    <u>INVOICE NUMBER :<br />
		                                                    </b>
			                                                    <span>{{INVOICE_NUMBER}}</span> <br />
			                                                    <span>DATE :</span> <br />
			                                                    <span>{{INVOICE_DATE}}</span> <br />
			                                                    <span>DUE DATE :</span> <br />
			                                                    <span>{{INVOICE_DUE_DATE}}</span> <br />
	                                                    </td> 
                                                    </tr>
                                                    <tr>
                                                    <td colspan='2'>
                                                    <hr />
                                                    </td>
                                                    </tr>
                                                    <tr>
                                                    <td colspan='2'>
                                                    {{PRODUCTS_DETAILS}}
                                                    </td>
                                                    </tr>
                                                    <tr>
                                                    <td colspan='2'>
                                                    <hr />
                                                    </td>
                                                    </tr>
                                                    <tr>
                                                    <td align='center' colspan='2'>
                                                    <p>
                                                    Powered by Headway Data Systems <br />
                                                    This invoice is a system generated and doesn't require any signature
                                                    </p>
                                                    </td>
                                                    </tr>
                                                    </table>
                                                    </td>
                                                    </tr>
                                                    </table>
                                                    </body>
                                                    </html>";
    }
}
