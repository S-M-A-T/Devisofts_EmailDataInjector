using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;
namespace Devisofts_EmailDataInjector
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Validate the number of parameters
                if (args.Length < 7) // Ensure there are at least 7 parameters
                {
                    Console.WriteLine("Invalid number of arguments. Please provide at least 7 parameters.");
                    Log("Invalid number of arguments.");
                    return;
                }

                // Retrieve parameters
                string tableType = args[0].Trim('"');
                string emailSubject = args[1].Trim('"');
                string emailInput = args[2].Trim('"');
                string header = args[3].Trim('"');
                string records = args[4].Trim('"');
                string cc = args[5].Trim('"');
                string message = args[6].Trim('"');
                string headercolor = args.Length > 7 ? args[7].Trim('"') : "#ffffff"; // Default header color if not provided

                // Debugging output
                Console.WriteLine("Table Type: " + tableType);
                Console.WriteLine("Email Subject: " + emailSubject);
                Console.WriteLine("Email Input: " + emailInput);
                Console.WriteLine("Header: " + header);
                Console.WriteLine("Records: " + records);
                Console.WriteLine("CC: " + cc);
                Console.WriteLine("Message: " + message);
                Console.WriteLine("Header Color: " + headercolor);

                // Split and validate email addresses
                List<string> emailList = emailInput.Split(';')
                                                    .Select(email => email.Trim())
                                                    .ToList();

                foreach (var email in emailList)
                {
                    if (!IsValidEmail(email))
                    {
                        Console.WriteLine("Invalid email address format: " + email);
                        Log("Invalid email address format: " + email);
                        return;
                    }
                }

                // Generate HTML table based on the table type
                string htmlTable;
                if (tableType.Equals("tabular", StringComparison.OrdinalIgnoreCase))
                {
                    htmlTable = GenerateHtmlTable(header, records, message, headercolor);
                }
                else if (tableType.Equals("singular", StringComparison.OrdinalIgnoreCase))
                {
                    htmlTable = GenerateSingularHtml(records, message, header, headercolor);
                }
                else
                {
                    Console.WriteLine("Invalid table type. Use 'tabular' or 'singular'.");
                    Log("Invalid table type.");
                    return;
                }

                // Send Email
                SENDMAIL(emailSubject, emailList, htmlTable, cc);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                Log("An error occurred: " + ex.Message);
            }

            Thread.Sleep(1000); // Sleep for 1000 milliseconds (1 second)

            // Terminate the program automatically without waiting for user input
            Environment.Exit(0);
        }

        public static void SENDMAIL(string SUBJECT, List<string> EMA, string MSG, string CC)
        {
            // Read email settings from the configuration file
            string mail_email = ConfigurationManager.AppSettings["mail_email"];
            string mail_pass = ConfigurationManager.AppSettings["mail_pass"];
            string mail_smtp = ConfigurationManager.AppSettings["mail_smtp"];
            int mail_smtpport = int.Parse(ConfigurationManager.AppSettings["mail_smtpport"]);
            bool enableSsl = bool.Parse(ConfigurationManager.AppSettings["SSL"]);

            SmtpClient client = new SmtpClient(mail_smtp, mail_smtpport)
            {
                EnableSsl = enableSsl
            };

            MailMessage message = new MailMessage
            {
                From = new MailAddress(mail_email, ConfigurationManager.AppSettings["sender_name"]),
                Subject = SUBJECT,
                Body = MSG,
                IsBodyHtml = true,
                Priority = MailPriority.High
            };

            // Add multiple recipients
            foreach (var email in EMA)
            {
                message.To.Add(email);
            }

            // Add CC emails if provided
            if (!string.IsNullOrEmpty(CC))
            {
                List<string> ccList = CC.Split(';').ToList();
                foreach (string ccEmail in ccList)
                {
                    message.CC.Add(ccEmail.Trim());
                }
            }

            NetworkCredential myCreds = new NetworkCredential(mail_email, mail_pass);
            client.Credentials = myCreds;

            try
            {
                Console.WriteLine("SMTP Server: " + mail_smtp + ", Port: " + mail_smtpport + ", Email: " + mail_email);
                Console.WriteLine("Sending email to: " + string.Join(", ", EMA));

                client.Send(message);
                Console.WriteLine("Email sent successfully!");
                Log("Email sent successfully to: " + string.Join(", ", EMA));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending email: " + ex.Message);
                Log("Error sending email: " + ex.Message);
            }
        }

        public static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        static string GenerateHtmlTable(string header, string records, string message, string hc)
        {
            // Split headers
            var headers = header.Split(',').Select(h => h.Trim()).ToArray(); // Change from '|' to ',' for headers

            // Split records using '^' for rows
            string[] recordEntries = records.Split(new[] { '^' }, StringSplitOptions.RemoveEmptyEntries);

            // Start building HTML table
            StringBuilder html = new StringBuilder();
            html.Append("<html><body><p>").Append(message).Append("</p><table border='1' style='border-collapse:collapse;'>");

            // Add headers
            html.Append("<tr>");
            foreach (var h in headers)
            {
                html.Append("<th style='padding: 10px 7px; border: 1px solid #9E9E9E; text-align: center; background:" + hc + "'>").Append(h).Append("</th>");
            }
            html.Append("</tr>");

            // Iterate over each record entry
            foreach (var record in recordEntries)
            {
                // Only process non-empty records
                if (string.IsNullOrWhiteSpace(record))
                {
                    Console.WriteLine("Skipping empty record.");
                    continue;
                }

                // Split fields by commas
                var fields = record.Split(',').Select(f => f.Trim()).ToArray();

                // Debugging output
                Console.WriteLine("Processing record: " + record);
                Console.WriteLine("Fields: " + string.Join(" | ", fields));

                // Check if the number of fields matches the header count
                if (fields.Length != headers.Length)
                {
                    Console.WriteLine("Warning: Record does not match header count. Skipping.");
                    continue; // Skip the record if it doesn't match
                }

                // Create a new row for the table
                html.Append("<tr>");
                for (int i = 0; i < fields.Length; i++)
                {
                    string str = fields[i].ToString();

                    var regx = new Regex("~");

                    if (regx.IsMatch(str))
                    {
                        string[] priceParts = fields[i].Split('~');

                        if (priceParts.Length > 1)
                        {
                            double amount; // Declare amount here

                            if (double.TryParse(priceParts[1].Replace(",", "").Trim(), out amount))
                            {
                                // Format the amount with commas
                                fields[i] = string.Format("{0:#,0}", amount); //+ "," + priceParts[1];
                            }
                            else
                            {
                                Console.WriteLine("Could not parse price: " + fields[i]);
                            }
                        }
                    }

                    html.Append("<td style='padding:5px;'>").Append(fields[i]).Append("</td>");
                }
                html.Append("</tr>");
            }

            html.Append("</table></body></html>");
            return html.ToString();
        }

        //static string GenerateSingularHtml(string records, string message, string notificationHeader, string headercolor)
        //{
        //    // Split individual records by '|', then split details within each record by '^'
        //    string[] recordEntries = records.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

        //    // Start building HTML content for singular format
        //    StringBuilder html = new StringBuilder();

        //    // Add the main title and additional header
        //    html.Append("<html><body>");
        //    html.Append("<p>").Append(message).Append("</p>");
        //    html.Append("<table border='1' style='border-collapse:collapse; width: 100%;'>");

        //    // Add header row for the title
        //    html.Append("<tr style='background-color: " + headercolor + ";'>");
        //    html.Append("<td colspan='2' style='padding: 10px; text-align: center; font-weight: bold;'>" + notificationHeader + "</td>");
        //    html.Append("</tr>");

        //    // Iterate through each record
        //    int recordNumber = 1;
        //    foreach (var record in recordEntries)
        //    {
        //        if (string.IsNullOrWhiteSpace(record))
        //        {
        //            continue;
        //        }

        //        // Add a distinct separator or heading for the new record
        //        html.Append("<tr style='background-color: lightgray;'>");
        //        html.Append("<td colspan='2' style='padding: 10px; font-weight: bold;'>Record #" + recordNumber + "</td>");
        //        html.Append("</tr>");

        //        // Split individual details in the record by '^'
        //        string[] details = record.Split(new[] { '^' }, StringSplitOptions.RemoveEmptyEntries);

        //        foreach (var detail in details)
        //        {
        //            string processedDetail = detail;

        //            // Handle '~' (tilde) formatting for amounts
        //            if (processedDetail.Contains("~"))
        //            {
        //                string[] parts = processedDetail.Split('~');
        //                double amount;

        //                // Check if the second part (amount) is a valid number and remove any redundant ':'
        //                if (parts.Length > 1 && double.TryParse(parts[1].Replace(",", "").Trim(), out amount))
        //                {
        //                    // Reformat the amount part with commas, but avoid adding extra ':' if already present
        //                    processedDetail = parts[0].Trim().Replace(":", "") + ": " + string.Format("{0:#,0}", amount);
        //                }
        //            }

        //            // Add a row for each detail in the record
        //            html.Append("<tr>");
        //            html.Append("<td style='padding: 10px;' colspan='2'>" + processedDetail.Trim() + "</td>");
        //            html.Append("</tr>");
        //        }

        //        // Increment record number for the next record
        //        recordNumber++;
        //    }

        //    html.Append("</table></body></html>");
        //    return html.ToString();
        //}

        static string GenerateSingularHtml(string records, string message, string notificationHeader, string headercolor)
        {
            // Split individual records by '|' to get the separate record blocks
            string[] recordEntries = records.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            // Start building HTML content for singular format
            StringBuilder html = new StringBuilder();

            // Add the main title and additional message
            html.Append("<html><body>");
            html.Append("<p style='font-weight: bold;'>" + message + "</p>");

            // Iterate through each record block and create separate tables for each
            foreach (var record in recordEntries)
            {
                if (string.IsNullOrWhiteSpace(record))
                {
                    continue;
                }

                // Add a div with margin to ensure spacing around the table
                html.Append("<div style='margin-top: 20px; '>"); // Adds space around the table

                // Add a spacer before the header
                html.Append("<div style='margin-top: 20px;'></div>"); // Space before the notification header
                html.Append("<br>"); // Space 
                // Add the table with border and auto width
                html.Append("<table border='1' style='border-collapse: collapse;'>");


                // Add header row for the title with specified background color
                html.Append("<tr style='background-color: " + headercolor + ";'>");
                html.Append("<td style=' text-align: center; font-weight: bold;'>" + notificationHeader + "</td>");
                html.Append("</tr>");

                // Split individual details in the record by '^'
                string[] details = record.Split(new[] { '^' }, StringSplitOptions.RemoveEmptyEntries);

                // Process each detail
                foreach (var detail in details)
                {
                    string processedDetail = detail.Trim();

                    // Handle '~' (tilde) formatting for amounts
                    if (processedDetail.Contains("~"))
                    {
                        string[] parts = processedDetail.Split('~');
                        double amount;

                        // Check if the second part (amount) is a valid number and remove any redundant ':'
                        if (parts.Length > 1 && double.TryParse(parts[1].Replace(",", "").Trim(), out amount))
                        {
                            // Reformat the amount part with commas, but avoid adding extra ':' if already present
                            processedDetail = parts[0].Trim().Replace(":", "") + ": " + string.Format("{0:#,0}", amount);
                        }
                    }

                    // Add a single row for each detail in the record
                    html.Append("<tr>");
                    html.Append("<td style='padding: 10px; white-space: nowrap;'>" + processedDetail + "</td>");
                    html.Append("</tr>");
                }

                html.Append("</table>");
                html.Append("</div>"); // Close the div for the table
            }

            html.Append("</body></html>");
            return html.ToString();
        }







        static void Log(string message)
        {
            string logFilePath = @"C:\Temp\EMTAB_LOG.txt"; // Define your log file path
            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine(DateTime.Now + ": " + message);
            }
        }


    }
}
