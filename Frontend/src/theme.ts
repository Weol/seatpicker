import { createTheme } from "@mui/material/styles"

// A custom theme for this app
const theme = createTheme({
  palette: {
    mode: "dark",
    primary: {
      main: "#ffffff",
    },
    secondary: {
      main: "#f1ff00",
    },
    background: {
      default: "#1b2838",
      paper: "#1b2838",
    },
  },
})

export default theme
