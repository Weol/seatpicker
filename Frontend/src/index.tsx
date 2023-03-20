import * as React from 'react';
import {createRoot} from 'react-dom/client';
import CssBaseline from '@mui/material/CssBaseline';
import {ThemeProvider} from '@mui/material/styles';
import App from './App';
import theme from './theme';
import {BrowserRouter} from 'react-router-dom';

const rootElement = document.getElementById('root');

if (rootElement != null) {
  const root = createRoot(rootElement);

  root.render(
    <ThemeProvider theme={theme}>
      {/* CssBaseline kickstart an elegant, consistent, and simple baseline to build upon. */}
      <CssBaseline/>
      <BrowserRouter>
        <App/>
      </BrowserRouter>
    </ThemeProvider>,
  );
}