import { Button, Stack } from "@mui/material"
import Typography from "@mui/material/Typography"

export default function ErrorPage() {
  const handleReload = () => {
    location.reload()
  }

  return (
    <Stack sx={{ my: 4, alignItems: "center" }}>
      <Typography variant="h1" component="h1" gutterBottom>
        Noe gikk galt 💀
      </Typography>
      <Typography variant="body1" component="h1" align="center" gutterBottom>
        Noe gikk fryktelig galt, venligst prøv på nytt
      </Typography>
      <Button variant="outlined" onClick={handleReload}>
        Last inn siden på nytt
      </Button>
    </Stack>
  )
}
