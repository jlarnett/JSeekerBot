using ClosedXML.Excel;
using JSeeker.Common.Services;
using Microsoft.Playwright;

namespace JSeekerBot
{
    [TestFixture]
    public class ApplyForJobsEasily : PageTest
    {
        private List<JobApplicationStatusEntry> _applicationStatusEntries = new List<JobApplicationStatusEntry>();
        private ResponseInterpreter _responseInterpreter;
        private SettingsBuilder.SettingsConfig _settingsConfig;

        [OneTimeSetUp]
        public void Setup()
        {
            _responseInterpreter = new ResponseInterpreter();
            _settingsConfig = SettingsLoader.LoadFile();
            var questionResponsePairs = SettingsLoader.LoadQuestionResponseFile();
            _responseInterpreter.AddQuestionResponse(questionResponsePairs);
        }

        [Test]
        public async Task ApplyNow()
        {
            //Navigate to LinkedIn site
            await Page.GotoAsync("https://linkedin.com/");

            //Login to LinkedIn profile
            await Page.LoginLinkedInAsync();


            //await Page.WaitForURLAsync("https://www.linkedin.com/feed/");


            //Navigate to the Find Jobs page. 
            await Page.GetByRole(AriaRole.Link, new () { NameString = "Jobs" }).First.ClickAsync();
            await Page.WaitForURLAsync("https://www.linkedin.com/jobs/");

            await Page.GetByLabel("Search by title, skill, or company").First.FillAsync(_settingsConfig.JobSearchRole);
            await Page.GetByLabel("Search by title, skill, or company").Last.PressAsync("Enter");

            await Page.WaitForTimeoutAsync(500);
            //await Page.Locator(".jobs-semantic-search-location-filter button").ClickAsync();
            await Page.WaitForTimeoutAsync(500);
            await Page.GetByLabel("City, state, or zip code").First.FillAsync(_settingsConfig.JobSearchLocation);
            await Page.WaitForTimeoutAsync(500);
            await Page.GetByLabel("City, state, or zip code").First.PressAsync("Enter");

            var currentPageCounter = 1;
            ILocator nextPageButton;
            var reachedEasyApplyLimit = false;

            int jobsProcessed = 0;
            int applicationsSubmitted = 0;

            //Probably need to find a better wait for this specific purpose. Waiting for Jobs List page to load -> slow load causing issues
            await Page.WaitForTimeoutAsync(10000);


            //Check each job in the job list. Look for easy apply button
            do
            {

                //Verify that the last job application form has closed, before starting another
                await CloseJobDetails();

                //List of job postings for the current page. Contains list of jobPosts
                var jobList = await Page.QuerySelectorAllAsync(".scaffold-layout__list-item");

                foreach (var jobPost in jobList)
                {
                    await jobPost.ClickAsync();

                    //Easy Apply Button locator. 
                    var easyApplyButton = Page.Locator(".jobs-apply-button").First;

                    //variable to check if our bot reached max apply limmit
                    reachedEasyApplyLimit = await Page
                        .GetByText("Easy Apply application limit").IsVisibleAsync();

                    //Breaks bot from going through rest of job posting page results. Breaks loop and signifies end of test. 
                    if (reachedEasyApplyLimit)
                    {
                        break;
                    }

                    if (await easyApplyButton.IsVisibleAsync())
                    {
                        if (await easyApplyButton.InnerTextAsync() == "Easy Apply")
                        {
                            //Button CONFIRMED to be Easy Apply -> Try to submit profiles application
                            await easyApplyButton.ClickAsync();
                            bool successfulApplication = await TryToSubmitApplication();

                            if (successfulApplication)
                                applicationsSubmitted++;

                            Console.WriteLine($"Applications Submitted - {applicationsSubmitted}");
                            // Page.WaitForTimeoutAsync(1500);
                        }
                        else
                        {
                            //Record the job posting into excel document.
                            _applicationStatusEntries.Add(await Page.GetApplicationStatusEntryDetailsAsync(false, false));
                        }
                    }

                    await Page.WaitForTimeoutAsync(150);

                    jobsProcessed++;
                    Console.WriteLine($"Jobs Processed - {jobsProcessed}");
                }

                //Increment page counter & click page counter to move bot to next page of results
                currentPageCounter++;
                nextPageButton = Page.Locator($"[data-test-pagination-page-btn='{currentPageCounter}']");
                await nextPageButton.ClickAsync();


            //If we run out of pages or reach the easy apply limit counter, we continue applying. Otherwise, shut down the bot successfully
            } while ((await nextPageButton.IsVisibleAsync() || await nextPageButton.IsEnabledAsync()) && !reachedEasyApplyLimit);
        }


        /// <summary>
        /// This function contains a loop to pass through the Easy Apply Pages
        /// Calls another function to try to fill the required inputs first. 
        /// Once the Review Application button is present clicks it, then submits the application.
        /// </summary>
        /// <returns></returns>
        private async Task<bool> TryToSubmitApplication()
        {
            int tryCounter = 0;
            bool successfullyApplied = false;

            var applicationStatusEntry = await Page.GetApplicationStatusEntryDetailsAsync(false, true);

            while (await Page.Locator($"[aria-label='Continue to next step']").CountAsync() > 0 && tryCounter < 15)
            {
                //Try to fill the current application forms page. Try 15 times before giving up. TODO - This could probably be lowered was a failsafe
                await TryFillEmptyInputs(applicationStatusEntry.JobTitle);
                if (await Page.Locator($"[aria-label='Continue to next step']").CountAsync() > 0 && await Page.Locator($"[aria-label='Continue to next step']").CountAsync() > 0 && await Page.Locator($"[aria-label='Continue to next step']").IsEnabledAsync())
                    await Page.Locator($"[aria-label='Continue to next step']").ClickAsync();

                tryCounter++;
            }


            if (tryCounter < 15)
            {
                //If we were able to get past every application page try to submit the job application
                await TryFillEmptyInputs(applicationStatusEntry.JobTitle);
                if (await Page.GetByRole(AriaRole.Button, new() { NameString = "Review your application" }).IsVisibleAsync())
                    await Page.GetByRole(AriaRole.Button, new () { NameString = "Review your application" }).ClickAsync();

                if (await Page.GetByRole(AriaRole.Button, new() { NameString = "Submit application" }).IsVisibleAsync())
                {
                    await Page.GetByRole(AriaRole.Button, new() { NameString = "Submit application" }).ClickAsync();
                    successfullyApplied = true;
                }

            }

            //Track the application submission success in the excel document
            applicationStatusEntry.ApplicationStatus = successfullyApplied;
            _applicationStatusEntries.Add(applicationStatusEntry);

            await CloseJobDetails();

            return successfullyApplied;
        }

        /// <summary>
        /// Attempts to fill the easy apply required fields. Currently just puts a 4 in any text input and selects yes to any dropdown.
        /// TODO: Implement a system of tags - value pairs to input more accurate information
        /// </summary>
        /// <returns></returns>
        private async Task TryFillEmptyInputs(string jobTitle)
        {
            await PopulateFormTextboxes(jobTitle);
            await PopulateFormDropdowns();
            await PopulateFormRadioButtons();
        }

        private async Task PopulateFormDropdowns()
        {
            //Get a list of all combo boxes on application form
            var comboBoxes = await Page.QuerySelectorAllAsync("select");

            //Iterate through each combobox
            foreach (var box in comboBoxes)
            {

                var options = await box.QuerySelectorAllAsync("option");

                //Get the textboxes question
                var inputId = await box.GetAttributeAsync("Id");

                var questionLabelElement = Page.Locator($"[for='{inputId}']");

                //Get the question from the application
                string question = "";

                if (await questionLabelElement.IsVisibleAsync())
                    question = await questionLabelElement.InnerTextAsync();


                //Check if we have a correct response in dictionary
                var response = _responseInterpreter.GetQuestionResponse(question);

                //Only enter response if it isn't prefilled already TODO - Maybe modify this to happen no matter what?
                if (await box.InputValueAsync() == "" || await box.InputValueAsync() == "Select an option")
                {
                    if (response != null)
                    {
                        var isInt = int.TryParse(response, out int result);

                        if (!isInt)
                        {
                            foreach (var option in options)
                            {
                                var optionValue = await option.GetAttributeAsync("value");
                                bool optionContainsText = optionValue.Contains(response);

                                if (optionContainsText)
                                    await box.SelectOptionAsync(optionValue);
                            }
                        }
                        else
                        {
                            await box.SelectOptionAsync(await options[1].GetAttributeAsync("value"));
                        }

                    }
                    else
                    {
                        int.TryParse(_settingsConfig.DefaultComboBoxResponse, out int result);
                        await box.SelectOptionAsync(await options[result].GetAttributeAsync("value") );
                    }
                }
            }
        }

        private async Task PopulateFormRadioButtons()
        {
            var radioButtonQuestionContainers = await Page.QuerySelectorAllAsync("fieldset[data-test-form-builder-radio-button-form-component='true']");

            foreach (var question in radioButtonQuestionContainers)
            {
                //Get the textboxes question

                var questionLabelElement = await question.QuerySelectorAsync("legend");

                string questionText = "";

                if (questionLabelElement != null)
                {
                    if (await questionLabelElement.IsVisibleAsync())
                        questionText = await questionLabelElement.InnerTextAsync();
                }

                var response = _responseInterpreter.GetQuestionResponse(questionText);
                var options = await question.QuerySelectorAllAsync("label");

                if (response != null)
                {
                    if (response == "yes" || response == "Yes")
                    {
                        await options[0].ClickAsync();
                    }
                    else
                    {
                        await options[1].ClickAsync();
                    }
                }
                else
                {
                    if (_settingsConfig.DefaultRadioButtonResponse.ToUpper() == "YES")
                    {
                        var option = options.FirstOrDefault();
                        if (option != null)
                            await option.ClickAsync();
                    }
                    else if (_settingsConfig.DefaultRadioButtonResponse.ToUpper() == "NO")
                    {
                        await options[1].ClickAsync();
                    }
                    else
                    {
                        foreach (var option in options)
                        {
                            if ((await option.TextContentAsync()).Contains(_settingsConfig.DefaultRadioButtonResponse))
                            {
                                await option.ClickAsync();
                            }
                        }
                    }
                }
            }
        }

        private async Task PopulateFormTextboxes(string jobTitle)
        {
            var inputs = await Page.QuerySelectorAllAsync(".artdeco-text-input--input");

            foreach (var input in inputs)
            {
                //Get the textboxes question
                var inputId = await input.GetAttributeAsync("Id");
                var questionLabelElement = Page.Locator($"[for='{inputId}']");

                string question = "";

                if (await questionLabelElement.IsVisibleAsync())
                    question = await questionLabelElement.InnerTextAsync();

                var response = _responseInterpreter.GetQuestionResponse(question, jobTitle);

                if (response != null)
                {
                    await input.FillAsync(response);
                }
                else
                {
                    await input.FillAsync(_settingsConfig.DefaultTextboxResponse);
                }
            }

            //Handle Basic Textboxes - Weird Location (City) dropdowns
            var basicTextInputs = await Page.QuerySelectorAllAsync(".jobs-easy-apply-modal__content .basic-input ");
            foreach (var input in basicTextInputs)
            {
                //Get the textboxes question
                var inputId = await input.GetAttributeAsync("Id");
                var questionLabelElement = Page.Locator($"[for='{inputId}']");

                string question = "";

                if (await questionLabelElement.IsVisibleAsync())
                    question = await questionLabelElement.InnerTextAsync();

                var response = _responseInterpreter.GetQuestionResponse(question, jobTitle);

                if (response != null)
                {
                    await input.FillAsync(response);
                    await input.PressAsync("ArrowDown");
                    await input.PressAsync("Enter");
                    
                }
                else
                {
                    await input.FillAsync(_settingsConfig.DefaultTextboxResponse);
                    await input.PressAsync("ArrowDown");
                    await input.PressAsync("Enter");
                }
            }
        }

        /// <summary>
        /// Closes out of the Easy Apply Form. Used when unable to submit, or after app submission.
        /// Used to keep flow when things don't work exactly as expected.
        /// </summary>
        /// <returns></returns>
        private async Task CloseJobDetails()
        {
            //var allDismisses = await Page.GetByRole(AriaRole.Button, new() { NameString = "Dismiss" }).AllAsync();

            //foreach (var element in allDismisses)
            //{
            //    if (await element.IsVisibleAsync() && await element.IsEnabledAsync())
            //        await element.ClickAsync();
            //}

            //var allDiscard = await Page.GetByRole(AriaRole.Button, new() { NameString = "Discard" }).AllAsync();

            //foreach (var element in allDiscard)
            //{
            //    if (await element.IsVisibleAsync() && await element.IsEnabledAsync())
            //        await element.ClickAsync();
            //}

            if (await Page.Locator(".artdeco-modal").GetByRole(AriaRole.Button, new() { NameString = "Dismiss" }).IsVisibleAsync())
                await Page.Locator(".artdeco-modal").GetByRole(AriaRole.Button, new() { NameString = "Dismiss" }).ClickAsync();

            if (await Page.Locator(".artdeco-modal--layer-confirmation ").GetByRole(AriaRole.Button, new() { NameString = "Discard" }).IsVisibleAsync())
                await Page.Locator(".artdeco-modal--layer-confirmation ").GetByRole(AriaRole.Button, new() { NameString = "Discard" }).ClickAsync();
        }

        [OneTimeTearDown]
        public void GenerateExcelDocument()
        {


            bool resultFileExists = File.Exists($"{_settingsConfig.ResultFolderPath}//Applications.xlsx");
            XLWorkbook wb;

            if (resultFileExists)
            {
                wb = new XLWorkbook($"{_settingsConfig.ResultFolderPath}//Applications.xlsx");
            }
            else
            {
                wb = new XLWorkbook();
            }

            var workSheet = wb.AddWorksheet($"Run-{DateTime.UtcNow.Ticks}");

            workSheet.Cell(1, 1).Value = "Company";
            workSheet.Cell(1, 2).Value = "Job Title";
            workSheet.Cell(1, 3).Value = "Date Application Attempted";
            workSheet.Cell(1, 4).Value = "Application Submitted";
            workSheet.Cell(1, 5).Value = "Had Easy Apply";
            workSheet.Cell(1, 6).Value = "Salary";

            for (int i = 0; i < _applicationStatusEntries.Count; i++)
            {
                workSheet.Cell(i+2, 1).Value = _applicationStatusEntries[i].CompanyName;
                workSheet.Cell(i+2, 2).Value = _applicationStatusEntries[i].JobTitle;
                workSheet.Cell(i+2, 3).Value = _applicationStatusEntries[i].DateAttempted;
                workSheet.Cell(i+2, 4).Value = _applicationStatusEntries[i].ApplicationStatus;
                workSheet.Cell(i+2, 5).Value = _applicationStatusEntries[i].HadEasyApplyButton;
                workSheet.Cell(i+2, 6).Value = _applicationStatusEntries[i].Salary;
            }

            wb.SaveAs($"{_settingsConfig.ResultFolderPath}//Applications.xlsx");
        }


    }

    public class JobApplicationStatusEntry()
    {

        public string CompanyName { get; set; }
        public bool ApplicationStatus { get; set; }
        public DateTime DateAttempted { get; set; }
        public string JobTitle { get; set; }
        public bool HadEasyApplyButton { get; set; }
        public string Salary { get; set; }

        public JobApplicationStatusEntry(string companyName, bool applicationStatus, DateTime dateAttempted, string jobTitle, bool hadEasyApply, string salary) : this()
        {
            this.CompanyName = companyName;
            this.ApplicationStatus = applicationStatus;
            this.DateAttempted = dateAttempted;
            this.JobTitle = jobTitle;
            this.HadEasyApplyButton = hadEasyApply;
            this.Salary = salary;
        }
    }
}
