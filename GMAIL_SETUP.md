# Gmail draft setup

Invoice Builder creates a Gmail draft and attaches the selected invoice PDFs and the generated statement PDF. It does not send the message. The first use opens Google's sign-in and consent flow; after authorization, the app saves its OAuth token under the current Windows user's `%AppData%\InvoiceBuilder\GoogleOAuth` folder.

## Configure Google OAuth

1. Create or select a project in [Google Cloud Console](https://console.cloud.google.com/).
2. Enable the Gmail API for that project.
3. Configure the OAuth consent screen. For a personal account, use the External audience and add the Gmail account as a test user while the app is in Testing.
4. Create an OAuth client ID with application type **Desktop app** and download its JSON file.
5. Rename the downloaded file to `credentials.json` and place it in the same folder as `InvoiceBuilder.exe`.
6. In Invoice Builder Admin Settings, set **Default Email Recipient**. Generate a statement and approve the Gmail permission prompt.

The credentials file is deliberately excluded from Git and is not bundled by the project. Keep it private and provide it separately when deploying the app.

## Google consent screen note

The app requests Google's `gmail.compose` scope so it can create drafts. Google classifies that scope as restricted. For personal use, an app can be kept in Testing with the intended account listed as a test user, but Google expires refresh-token authorizations for External apps in Testing after seven days. You may need to authorize again weekly. Removing that Testing limitation for broader distribution involves Google's OAuth verification process.
