import {createTheme} from '@mui/material/styles';

// A custom theme for this app
const theme = createTheme({
  palette: {
    primary: {
      main: '#ffff00',
    },
    secondary: {
      main: '#ffff00',
    },
    background: {
      default: '#2b2a33',
      paper: '#42414d',
    },
    text: {
      primary: 'rgba(255,255,255,0.9)',
      secondary: 'rgba(255,255,255,0.7)',
      disabled: 'rgba(255,255,255,0.5)',
    },
  }
});

export default theme;
