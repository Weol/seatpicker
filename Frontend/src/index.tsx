import { AppInsightsErrorBoundary, ReactPlugin } from "@microsoft/applicationinsights-react-js"
import { ApplicationInsights } from "@microsoft/applicationinsights-web"
import CssBaseline from "@mui/material/CssBaseline"
import { ThemeProvider } from "@mui/material/styles"
import { createRoot } from "react-dom/client"
import { BrowserRouter } from "react-router-dom"
import { RecoilRoot } from "recoil"
import App from "./App"
import ErrorPage from "./Pages/ErrorPage"
import theme from "./theme"
import AuthErrorBoundary from "./AuthErrorBoundry"
import React from "react"

const rootElement = document.getElementById("root")

if (rootElement != null) {
  const root = createRoot(rootElement)

  const reactPlugin = new ReactPlugin()
  const appInsights = new ApplicationInsights({
    config: {
      connectionString:
        "InstrumentationKey=0795c10c-534e-4440-b5b8-a02a27fc86c0;IngestionEndpoint=https://norwayeast-0.in.applicationinsights.azure.com/;LiveEndpoint=https://norwayeast.livediagnostics.monitor.azure.com/",
      enableAutoRouteTracking: true,
      extensions: [reactPlugin],
    },
  })
  appInsights.loadAppInsights()

  root.render(
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <BrowserRouter>
        <RecoilRoot>
          <AppInsightsErrorBoundary onError={() => <ErrorPage />} appInsights={reactPlugin}>
            <AuthErrorBoundary>
              <App />
            </AuthErrorBoundary>
          </AppInsightsErrorBoundary>
        </RecoilRoot>
      </BrowserRouter>
    </ThemeProvider>
  )
}
