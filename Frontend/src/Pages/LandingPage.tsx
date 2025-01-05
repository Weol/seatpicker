import { Button, Card, Stack } from "@mui/material"
import Typography from "@mui/material/Typography"
import React from "react"
import { Link } from "react-router-dom"

export default function LandingPage() {
  return (
    <Stack sx={{ alignItems: "center" }}>
      <Typography variant="h5" component="h1" gutterBottom>
        Velkommen til
      </Typography>
      <Typography variant="h1" fontSize={{ xs: 50, sm: 68 }} component="h1" gutterBottom>
        SaltenLAN
      </Typography>
      <Typography variant="h5" component="h1" gutterBottom>
        på Drag i Hamarøy
      </Typography>
      <Typography marginTop="1em" variant="h5" component="h1" gutterBottom>
        17. til 19. januar
      </Typography>
      <Button component={Link} to="/reserve" color="secondary" variant={"contained"}>
        Resever en plass
      </Button>
      <Stack direction={"row"}>
        <Card>
          <Typography>Hehe</Typography>
        </Card>
        <Card>
          <Typography>Hehe</Typography>
        </Card>
        <Card>
          <Typography>Hehe</Typography>
        </Card>
      </Stack>
    </Stack>
  )
}
