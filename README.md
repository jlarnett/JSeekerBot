# JSeekerBot

JSeekerBot is built to simplify the process of applying to LinkedIn job postings. Anyone who regularly uses linkedIn has seen "Easy Apply" job postings with hundreds of applicants just minutes after becoming available.
JSeekerBot hopes to bring that power to your fingertips. The application was built using microsoft playwright and combines language processing with programmatic automation to submit countless job applications with accurate user information.

## Usage

1. The user needs to create a .env file inside of the JSeekerBot\JSeekerBot directory that contains valid LinkedIn user credentials. The example image below shows how the .env file should be formatted to work with the program correctly.
![image](https://github.com/user-attachments/assets/a1bbbd94-4070-48e9-b180-93dd4910c01c)

2. The user will need to swap out the question - response pairs contained in the Apply.cs setup function to valid information for the expected user / jobs.
![image](https://github.com/user-attachments/assets/6e740dc3-6d78-45c7-98ab-847c7778aa0a)

3. The program can be ran from Visual Studios Test Explorer or by running the command `dotnet tests --settings:chrome.runsettings`
