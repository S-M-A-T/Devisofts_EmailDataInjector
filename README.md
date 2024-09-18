<h1>Devisofts Email Data Injector</h1>

<p>The <strong>Devisofts Email Data Injector</strong> is a command-line application designed to facilitate the sending of formatted HTML emails with customizable content. It takes multiple parameters, processes the data, and constructs emails that can be sent to specified recipients. This tool is particularly useful for automating the email notification process based on dynamic data input.</p>

<h2>Features</h2>
<ul>
    <li><strong>HTML Email Generation:</strong> Supports both tabular and singular formats for email content.</li>
    <li><strong>Email Validation:</strong> Ensures that all provided email addresses are in valid format.</li>
    <li><strong>Customizable Headers:</strong> Allows for customizable email subject, header, and body message.</li>
    <li><strong>CC Support:</strong> Capability to add CC recipients for email notifications.</li>
    <li><strong>Logging:</strong> Logs important events and errors for debugging and tracking.</li>
</ul>

<h2>Usage</h2>
<pre>
<code>
dotnet Devisofts_EmailDataInjector.dll &lt;tableType&gt; &lt;emailSubject&gt; &lt;emailInput&gt; &lt;header&gt; &lt;records&gt; &lt;cc&gt; &lt;message&gt; [&lt;headercolor&gt;]
</code>
</pre>

<h2>Parameters</h2>
<ul>
    <li><strong>tableType:</strong> Specify the type of table format ('tabular' or 'singular').</li>
    <li><strong>emailSubject:</strong> Subject line for the email.</li>
    <li><strong>emailInput:</strong> Comma-separated list of recipient email addresses.</li>
    <li><strong>header:</strong> Header text for the email.</li>
    <li><strong>records:</strong> Data records formatted as per the chosen table type.</li>
    <li><strong>cc:</strong> (Optional) Comma-separated list of CC email addresses.</li>
    <li><strong>message:</strong> Body message to be included in the email.</li>
    <li><strong>headercolor:</strong> (Optional) Hex color code for the header background (default is #ffffff).</li>
</ul>

<h2>Requirements</h2>
<ul>
    <li><strong>.NET Framework:</strong> Requires .NET Framework to run.</li>
    <li><strong>Configuration:</strong> SMTP settings must be provided in the configuration file.</li>
</ul>

<h2>Installation</h2>
<p>To use the Devisofts Email Data Injector, clone the repository and build the project using Visual Studio or the .NET CLI. Ensure you have the correct SMTP settings configured in your application settings.</p>

<h2>License</h2>
<p>This project is licensed under the MIT License - see the <a href="LICENSE">LICENSE</a> file for details.</p>

<h2>Contributing</h2>
<p>Contributions are welcome! Please feel free to submit a pull request or open an issue for any enhancements or bugs.</p>

<h2>Contact</h2>
<p>For any questions or inquiries, please reach out to Support@devisofts.com.</p>
