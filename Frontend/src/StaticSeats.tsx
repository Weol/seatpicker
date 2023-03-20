import Seat from "./Models/Seat";

interface SeatReference {
  Id: string | null;
  Title: string | null;
  Width: number;
  Height: number;
  Y: number;
  X: number;
}

const createRow = (width: number, height: number, x: number, y: number, n: number, xOffset: number): Array<SeatReference> => {
  let row = []
  for (var i = 0; i < n; i++) {
    row.push({
      Id: null,
      Title: null,
      Y: y,
      X: x + (width + xOffset) * i,
      Width: width,
      Height: height
    })
  }
  return row
}

export default function createSeats(): { [Id: string]: Seat } {
  const tableWidth = 11.5
  const tableHeight = 2.81

  const offset = 0

  var x = 38 + 3.8
  var y = 36
  var yOffset = tableHeight * 2 + 5
  const rows: Array<Array<SeatReference>> = []

  rows.push(createRow(tableWidth, tableHeight, x, y, 5, offset))
  rows.push(createRow(tableWidth, tableHeight, x, y + tableHeight, 5, offset))

  y += yOffset
  rows.push(createRow(tableWidth, tableHeight, x, y, 5, offset))
  rows.push(createRow(tableWidth, tableHeight, x, y + tableHeight, 5, offset))

  y += yOffset
  rows.push(createRow(tableWidth, tableHeight, x, y, 5, offset))
  rows.push(createRow(tableWidth, tableHeight, x, y + tableHeight, 5, offset))

  y += yOffset
  rows.push(createRow(tableWidth, tableHeight, x, y, 5, offset))
  rows.push(createRow(tableWidth, tableHeight, x, y + tableHeight, 5, offset))

  rows[0][0].Id = "1052ccf8-3d0f-43e5-b595-6d770956d72c";
  rows[0][1].Id = "a4c5d446-9fa3-44b8-ae97-ed699be50009";
  rows[0][2].Id = "a56ba935-f0a8-47e4-bb32-e8dc94f1f9a5";
  rows[0][3].Id = "cbeeade4-49ab-44fb-b40b-e90081f9ad24";
  rows[0][4].Id = "bda04aeb-15a0-4f51-aaf1-748a84153b64";
  rows[1][0].Id = "781b585a-dbad-4c8b-93eb-09dc3d0f7706";
  rows[1][1].Id = "cff26494-d85e-4e44-8623-eea3482f3e31";
  rows[1][2].Id = "c536fc3b-2bc7-4ee1-8e9b-00a79e3dbc66";
  rows[1][3].Id = "ccc1ea18-ef3f-4152-99b9-dfcd9e9e2ef9";
  rows[1][4].Id = "9f2ce1a0-dc2c-49af-925e-c96df97d6881";
  rows[2][0].Id = "1525a4d1-e651-48a7-b439-4ce72d15183b";
  rows[2][1].Id = "5766ef80-3f36-497c-8284-eeae2b5ee7b2";
  rows[2][2].Id = "bb08f28a-c2b9-4ee3-99ae-4794f6ebca55";
  rows[2][3].Id = "ba3206d1-e188-4158-b894-78fa3e5a7d1b";
  rows[2][4].Id = "9eb5776d-6c15-4f9a-9501-a4c657984157";
  rows[3][0].Id = "e6b2f060-7aa3-4fd9-b57a-dd9b648e0c01";
  rows[3][1].Id = "15a1fbd6-e93e-46d1-96a5-8e89d927419c";
  rows[3][2].Id = "cda49386-eb8c-1da2-8096-a742b1a92dd5";
  rows[3][3].Id = "cdc49386-eb8c-4da2-8096-a742baa92dd5";
  rows[3][4].Id = "3a8e474a-aa8d-43de-bd74-7169c56b1c89";
  rows[4][0].Id = "349f04f3-31c6-41c9-8e5c-ecf787272344";
  rows[4][1].Id = "7c7f118b-adab-4c8f-abaf-3e6404e02308";
  rows[4][2].Id = "cc01dfe8-ad67-4863-87f3-426d8fc1eabd";
  rows[4][3].Id = "be265bb6-966c-45c3-897b-6adcf38705fd";
  rows[4][4].Id = "1d6b2a97-8413-48b2-9892-ef6e2f54f722";
  rows[5][0].Id = "5b08e098-bdd8-43e2-bb86-e94bd749431c";
  rows[5][1].Id = "a7ac7c8c-1fe7-45f6-a9c0-a9a4ffea5857";
  rows[5][2].Id = "e0bc0f28-7995-43cb-9b6f-5b500bb7c43d";
  rows[5][3].Id = "3114a607-b634-4995-998b-a481c2fe69ea";
  rows[5][4].Id = "4bd4a8f9-41d7-4e82-b4f7-483a92091bbe";
  rows[6][0].Id = "5d41ddce-6acc-497e-b2b0-1bdbe1f3a058";
  rows[6][1].Id = "5f08943a-8689-4839-9d82-fa3c3b512bc5";
  rows[6][2].Id = "a4c5d446-9fa3-44b8-ae97-ed699be50209";
  rows[6][3].Id = "e6b2f060-7aa3-4fd9-b57a-dd9b648e0201";
  rows[6][4].Id = "15a1fbd6-e93e-46d1-96a5-8e89d927429c";
  rows[7][0].Id = "6afc155a-f2b8-4943-91e1-4dec0b83cfa0";
  rows[7][1].Id = "22606245-41c3-407d-b341-ec7f4d07f279";
  rows[7][2].Id = "08d3d22a-87ab-45c5-86f5-6a8aaf1ebfb1";
  rows[7][3].Id = "728b21e9-cb5f-41d6-9352-99b7b58d0fe5";
  rows[7][4].Id = "2052ccf8-3d0f-43e5-b595-6d770956d72c";

  var seats: { [Id: string]: Seat } = {}

  var title = 0
  rows.forEach(row => {
    row.forEach(seat => {
      title++
      seats[seat.Id ?? "NO ID"] = {
        id: seat.Id ?? "NO ID",
        user: null,
        title: title + "",
        width: seat.Width,
        height: seat.Height,
        y: seat.Y,
        x: seat.X,
      }
    })
  })

  console.log(JSON.stringify(Object.values(seats)))

  return seats
}