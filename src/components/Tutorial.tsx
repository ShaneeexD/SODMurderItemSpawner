import React from 'react';
import { 
  Typography, 
  Paper, 
  Box, 
  Divider,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  Tab,
  Tabs
} from '@mui/material';
import InfoIcon from '@mui/icons-material/Info';
import SettingsIcon from '@mui/icons-material/Settings';
import LocationOnIcon from '@mui/icons-material/LocationOn';
import EventIcon from '@mui/icons-material/Event';
import InventoryIcon from '@mui/icons-material/Inventory';
import PersonIcon from '@mui/icons-material/Person';
import CodeIcon from '@mui/icons-material/Code';

import { Prism as SyntaxHighlighter } from 'react-syntax-highlighter';
import { vscDarkPlus } from 'react-syntax-highlighter/dist/esm/styles/prism';

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;

  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`simple-tabpanel-${index}`}
      aria-labelledby={`simple-tab-${index}`}
      {...other}
    >
      {value === index && (
        <Box sx={{ p: 3 }}>
          {children}
        </Box>
      )}
    </div>
  );
}

// Example JSON configurations
const exampleConfigs = {
  homeWorkplace: {
    name: "Home/Workplace",
    description: "Spawns an item at the home or workplace of the recipient.",
    json: `{
  "SpawnRules": [
    {
      "Name": "TutorialExample",
      "Enabled": true,
      "TriggerEvents": [
        "PickNewVictim"
      ],
      "MurderMO": "TheDoveKiller",
      "ItemToSpawn": "ExampleCustomItem",
      "SpawnChance": 1,
      "SpawnLocation": 8,
      "BelongsTo": 0,
      "Recipient": 1,
      "UseFurniture": true,
      "FurniturePresets": [
        "KitchenCounter",
        "KitchenCounterSmall"
      ],
      "CustomRoomName": "Kitchen"
    }
  ]
}`
  },
  mailbox: {
    name: "Mailbox",
    description: "Spawns an item at the mailbox of the recipient.",
    json: `{
  "SpawnRules": [
    {
      "Name": "TutorialExample",
      "Enabled": true,
      "TriggerEvents": [
        "OnVictimKilled"
      ],
      "MurderMO": "TheDoveKiller",
      "ItemToSpawn": "ExampleCustomItem",
      "SpawnChance": 1,
      "SpawnLocation": 0,
      "BelongsTo": 0,
      "Recipient": 0,
      "UnlockMailbox": true
    }
  ]
}`
  },
  homeWorkplaceBuildingEntrance: {
    name: "Home/Workplace Building Entrance",
    description: "Spawns an item at the home or workplace building entrance of the recipient.",
    json: `{
  "SpawnRules": [
    {
      "Name": "TutorialExample",
      "Enabled": true,
      "TriggerEvents": [
        "PickNewVictim"
      ],
      "MurderMO": "TheDoveKiller",
      "ItemToSpawn": "ExampleCustomItem",
      "SpawnChance": 1,
      "SpawnLocation": 4,
      "BelongsTo": 0,
      "Recipient": 1,
      "SubLocationTypeBuildingEntrances": 1
    }
  ]
}`
  },
  custom: {
    name: "Custom Location",
    description: "Spawning an item at a custom location using specific rooms and furniture.",
    json: `
    EXAMPLE 1:

{
  "SpawnRules": [
    {
      "Name": "TutorialExample",
      "Enabled": true,
      "TriggerEvents": [
        "OnVictimKilled"
      ],
      "MurderMO": "TheScammer",
      "ItemToSpawn": "BriefcaseDocuments",
      "SpawnChance": 1,
      "SpawnLocation": 11,
      "BelongsTo": 0,
      "Recipient": 1,
      "UseFurniture": true,
      "FurniturePresets": [
        "FilingCabinet",
        "Shelf",
        "ShortFilingCabinet"
      ],
      "CustomRoomNames": [
        "Reception"
      ],
      "CustomRoomPresets": [
        "Reception"
      ],
      "CustomSubRoomNames": [
        "Backroom"
      ],
      "CustomSubRoomPresets": [
        "BusinessBackroom"
      ],
      "CustomFloorNames": [
        "CityHall_GroundFloor"
      ]
    }
  ]
}
  

EXAMPLE 2:

{
  "SpawnRules": [
    {
      "Name": "TutorialExample",
      "Enabled": true,
      "TriggerEvents": [
        "OnVictimKilled"
      ],
      "MurderMO": "TheDoveKiller",
      "ItemToSpawn": "BasBouleBat",
      "SpawnChance": 1,
      "SpawnLocation": 11,
      "BelongsTo": 0,
      "Recipient": 1,
      "UseFurniture": true,
      "FurniturePresets": [
        "KitchenCounter",
        "VintageFridge",
        "KitchenCounterSmall",
        "KitchenCounterSmallNoDrawers",
        "KitchenSink"
      ],
      "CustomRoomNames": [
        "Fast Food Restaurant"
      ],
      "CustomRoomPresets": [
        "FastFoodDiningRoom"
      ],
      "CustomSubRoomNames": [
        "Kitchen"
      ],
      "CustomSubRoomPresets": [
        "EateryKitchen"
      ],
      "CustomFloorNames": [
        "Eden_GroundFloor",
        "SherryFloor-1EateryRetail",
        "SherryFloor-2Retail1OfficeBeta",
        "OFA_GroundFloor-2Eatery",
        "MixedIndustrial_ground01",
        "OFA_GroundFloor-1Eatery1Retail"
      ]
    }
  ]
}`
  }
};

const Tutorial: React.FC = () => {
  const [tabValue, setTabValue] = React.useState(0);

  const handleTabChange = (_: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
  };
  
  return (
    <Box sx={{ width: '100%', maxWidth: '800px', margin: '0 auto', padding: '20px' }}>
      <Paper elevation={3} sx={{ p: 3, mb: 4, width: '100%' }}>
        <Typography variant="h4" component="h1" gutterBottom align="center">
          MurderItemSpawner Basic Tutorial
        </Typography>
        <Typography variant="body1" paragraph>
          Welcome to the MurderItemSpawner Config Generator! This tool helps you create configuration files for the MurderItemSpawner mod for Shadows Of Doubt.
        </Typography>
        <Divider sx={{ my: 3 }} />
        
        <Typography variant="h5" gutterBottom>
          Getting Started
        </Typography>
        <Typography variant="body1" paragraph>
          The MurderItemSpawner mod allows you to configure custom item spawns that trigger during specific game events. This tool makes it easy to create and manage these spawn configurations.
        </Typography>
        
        <List>
          <ListItem>
            <ListItemIcon>
              <InfoIcon color="primary" />
            </ListItemIcon>
            <ListItemText 
              primary="Basic Information" 
              secondary="Start by giving your spawn rule a unique name and description. This helps you identify the rule later." 
            />
          </ListItem>
          
          <ListItem>
            <ListItemIcon>
              <EventIcon color="primary" />
            </ListItemIcon>
            <ListItemText 
              primary="Event Trigger Settings" 
              secondary="Choose which game events will trigger your item spawn. You can select multiple events if needed." 
            />
          </ListItem>
          
          <ListItem>
            <ListItemIcon>
              <InventoryIcon color="primary" />
            </ListItemIcon>
            <ListItemText 
              primary="Item Settings" 
              secondary="Select which item to spawn and set the spawn chance (0.0 to 1.0, where 1.0 is 100%)." 
            />
          </ListItem>
          
          <ListItem>
            <ListItemIcon>
              <LocationOnIcon color="primary" />
            </ListItemIcon>
            <ListItemText 
              primary="Location Settings" 
              secondary="Define where the item should spawn. Options include specific rooms, random locations, or furniture." 
            />
          </ListItem>
          
          <ListItem>
            <ListItemIcon>
              <PersonIcon color="primary" />
            </ListItemIcon>
            <ListItemText 
              primary="Ownership Settings" 
              secondary="Determine who the item belongs to (fingerprints) and who the recipient is (In cases such as home, if you make the recipient the victim, it will spawn at their home)." 
            />
          </ListItem>
          
          <ListItem>
            <ListItemIcon>
              <CodeIcon color="primary" />
            </ListItemIcon>
            <ListItemText 
              primary="JSON Output" 
              secondary="After configuring all settings, the tool generates a JSON configuration that you can save and use with the mod." 
            />
          </ListItem>
        </List>
        
        <Divider sx={{ my: 3 }} />
        
        <Typography variant="h5" gutterBottom>
          Advanced Usage
        </Typography>
        <Typography variant="body1" paragraph>
          For advanced users, you can:
        </Typography>
        <List>
          <ListItem>
            <ListItemIcon>
              <SettingsIcon color="primary" />
            </ListItemIcon>
            <ListItemText 
              primary="Load Existing Configurations" 
              secondary="Use the 'Load JSON' button to load and edit existing configuration files." 
            />
          </ListItem>
        </List>
        
        <Box sx={{ mt: 4 }}>
          <Typography variant="h5" gutterBottom>
            Example Use Cases
          </Typography>
          <Typography variant="body1" paragraph>
            Here are some common use cases with example configurations:
          </Typography>
          
          <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
            <Tabs value={tabValue} onChange={handleTabChange} aria-label="example configurations">
              <Tab label="Home/Workplace" />
              <Tab label="Mailbox" />
              <Tab label="Home/Workplace Building Entrance" />
              <Tab label="Custom" />
            </Tabs>
          </Box>
          
          <TabPanel value={tabValue} index={0}>
            <Typography variant="h6">Home/Workplace</Typography>
            <Typography variant="body2" paragraph>Spawning an item in the recipient's home or workplace.</Typography>
            
            <Typography variant="subtitle1" sx={{ mt: 2, mb: 1 }}>How it works:</Typography>
            <List dense>
              <ListItem>
                <ListItemIcon><InfoIcon color="primary" fontSize="small" /></ListItemIcon>
                <ListItemText primary="TriggerEvents defines when the item should spawn (PickNewVictim event)" />
              </ListItem>
              <ListItem>
                <ListItemIcon><LocationOnIcon color="primary" fontSize="small" /></ListItemIcon>
                <ListItemText primary="SpawnLocation value 8 represents a home location, 9 represents a workplace location" />
              </ListItem>
              <ListItem>
                <ListItemIcon><PersonIcon color="primary" fontSize="small" /></ListItemIcon>
                <ListItemText primary="BelongsTo value 0 represents the murderer, 1 represents the victim and so on, these define whos fingerprints the item will have" />
              </ListItem>
              <ListItem>
                <ListItemIcon><InfoIcon color="primary" fontSize="small" /></ListItemIcon>
                <ListItemText primary="If furniture is true, the item will spawn on a furniture item, otherwise it will spawn at a random floor node within the address" />
              </ListItem>
              <ListItem>
                <ListItemIcon><InfoIcon color="primary" fontSize="small" /></ListItemIcon>
                <ListItemText primary="CustomRoomName is used to further narrow down the location if you want a specific room, it is optional" />
              </ListItem>
              <ListItem>
                <ListItemIcon><InfoIcon color="primary" fontSize="small" /></ListItemIcon>
                <ListItemText primary="CustomRoomName works by the address name prefix within the game, for example '804 Chandlers Heights' is the address, if you wanted to specify the bedroom, simply use 'Bedroom' the mod will automatically find the correct room name using the address prefix" />
              </ListItem>
              <ListItem>
                <ListItemIcon><InfoIcon color="primary" fontSize="small" /></ListItemIcon>
                <ListItemText primary="If furniture is true and no custom room is specified, it will search the entire address for a matching furniture item, if it cannot find one it will spawn at a random floor node within the address" />
              </ListItem>
              <ListItem>
                <ListItemIcon><InfoIcon color="primary" fontSize="small" /></ListItemIcon>
                <ListItemText primary="If furniture is true and a custom room is specified, it will search the custom room for a matching furniture item, if it cannot find one it will spawn at a random floor node within the address and not search the other rooms" />
              </ListItem>
            </List>
            
            <Box sx={{ mt: 2 }}>
              <SyntaxHighlighter language="json" style={vscDarkPlus} wrapLongLines>
                {exampleConfigs.homeWorkplace.json}
              </SyntaxHighlighter>
            </Box>
          </TabPanel>
          
          <TabPanel value={tabValue} index={1}>
            <Typography variant="h6">Mailbox</Typography>
            <Typography variant="body2" paragraph>Spawning an item in the mailbox of the recipient.</Typography>
            
            <Typography variant="subtitle1" sx={{ mt: 2, mb: 1 }}>How it works:</Typography>
            <List dense>
              <ListItem>
                <ListItemIcon><EventIcon color="primary" fontSize="small" /></ListItemIcon>
                <ListItemText primary="Triggers when a new victim is killed (OnVictimKilled event)" />
              </ListItem>
              <ListItem>
                <ListItemIcon><LocationOnIcon color="primary" fontSize="small" /></ListItemIcon>
                <ListItemText primary="Uses the SpawnLocation value 0 for mailbox" />
              </ListItem>
              <ListItem>
                <ListItemIcon><InventoryIcon color="primary" fontSize="small" /></ListItemIcon>
                <ListItemText primary="Specifies a specific item (ExampleCustomItem) to spawn" />
              </ListItem>
              <ListItem>
                <ListItemIcon><PersonIcon color="primary" fontSize="small" /></ListItemIcon>
                <ListItemText primary="Sets both BelongsTo (fingerprints) and Recipient (who the item is for) to murderer (0) so the item has the murderers fingerprints and will appear in their mailbox" />
              </ListItem>
              <ListItem>
                <ListItemIcon><InfoIcon color="primary" fontSize="small" /></ListItemIcon>
                <ListItemText primary="UnlockMailbox is true so the mailbox will be unlocked upon spawning the item, if false the mailbox will be locked with the item inside" />
              </ListItem>
            </List>
            
            <Box sx={{ mt: 2 }}>
              <SyntaxHighlighter language="json" style={vscDarkPlus} wrapLongLines>
                {exampleConfigs.mailbox.json}
              </SyntaxHighlighter>
            </Box>
          </TabPanel>
          
          <TabPanel value={tabValue} index={2}>
            <Typography variant="h6">Home/Workplace Building Entrance</Typography>
            <Typography variant="body2" paragraph>Spawning an item at the home or workplace building entrance of the recipient.</Typography>
            
            <Typography variant="subtitle1" sx={{ mt: 2, mb: 1 }}>How it works:</Typography>
            <List dense>
              <ListItem>
                <ListItemIcon><EventIcon color="primary" fontSize="small" /></ListItemIcon>
                <ListItemText primary="Triggers when a new victim is killed (OnVictimKilled event)" />
              </ListItem>
              <ListItem>
                <ListItemIcon><LocationOnIcon color="primary" fontSize="small" /></ListItemIcon>
                <ListItemText primary="Uses the SpawnLocation value 4 for home building entrance, use 5 for workplace building entrance" />
              </ListItem>
              <ListItem>
                <ListItemIcon><InventoryIcon color="primary" fontSize="small" /></ListItemIcon>
                <ListItemText primary="SubLocationTypeBuildingEntrances 0 spawns the item inside the entrance, 1 spawns the item outside the entrance" />
              </ListItem>
              <ListItem>
                <ListItemIcon><PersonIcon color="primary" fontSize="small" /></ListItemIcon>
                <ListItemText primary="BelongsTo value 0 represents the murderer, 1 represents the victim and so on, these define whos fingerprints the item will have" />
              </ListItem>
              <ListItem>
                <ListItemIcon><InfoIcon color="primary" fontSize="small" /></ListItemIcon>
                <ListItemText primary="Recipient defines who the item is for, and so whos home/workplace building entrance it spawns in" />
              </ListItem>
            </List>
            
            <Box sx={{ mt: 2 }}>
              <SyntaxHighlighter language="json" style={vscDarkPlus} wrapLongLines>
                {exampleConfigs.homeWorkplaceBuildingEntrance.json}
              </SyntaxHighlighter>
            </Box>
          </TabPanel>
          
          <TabPanel value={tabValue} index={3}>
            <Typography variant="h6">Custom Location</Typography>
            <Typography variant="body2" paragraph>Spawning an item at a custom location using specific rooms, floors and furniture.</Typography>
            
            <Typography variant="subtitle1" sx={{ mt: 2, mb: 1 }}>How it works:</Typography>
            <List dense>
              <ListItem>
                <ListItemIcon><EventIcon color="primary" fontSize="small" /></ListItemIcon>
                <ListItemText primary="Triggers when a victim is killed (OnVictimKilled event)" />
              </ListItem>
              <ListItem>
                <ListItemIcon><LocationOnIcon color="primary" fontSize="small" /></ListItemIcon>
                <ListItemText primary="Uses custom room, floor and furniture for precise placement" />
              </ListItem>
              <ListItem>
                <ListItemIcon><InventoryIcon color="primary" fontSize="small" /></ListItemIcon>
                <ListItemText primary="Spawns a briefcase with documents" />
              </ListItem>
              <ListItem>
                <ListItemIcon><PersonIcon color="primary" fontSize="small" /></ListItemIcon>
                <ListItemText primary="BelongsTo value 0 means the item belongs to the murderer, recipient does not matter in cases of custom locations" />
              </ListItem>
              <ListItem>
                <ListItemIcon><InfoIcon color="primary" fontSize="small" /></ListItemIcon>
                <ListItemText primary="First we have defined the floor as 'CityHall_GroundFloor' which will only find rooms within this floor preset, in cases like this, the ground floor for the City Hall is always the same, some buildings the floors may vary, and any floor may use different floor presets within different saves, you may want to include all types of floors for these or simply leave it empty" />
              </ListItem>
              <ListItem>
                <ListItemIcon><InfoIcon color="primary" fontSize="small" /></ListItemIcon>
                <ListItemText primary="Next we have defined the CustomRoomNames as 'Reception' and also the preset name, so right now we found the CityHall_GroundFloor, and now are looking for any room with 'Reception' matching in the name and preset, in this case it will find the ground floor enforcers office reception" />
              </ListItem>
              <ListItem>
                <ListItemIcon><InfoIcon color="primary" fontSize="small" /></ListItemIcon>
                <ListItemText primary="Next we have defined the CustomSubRoomNames as 'Backroom' and the preset as 'BusinessBackroom', this will find the backroom in the business address we found, using the company name as the prefix. This is not neccessary in this case as we can simply search for 'Backroom' in our CustomRoomName as we know no others will generate within the City Hall ground floor, but it will be handy for other places where there are multiple of the same room names such as 'Bathroom', we use the first CustomRoomName to find the business/appartment we want to spawn the item in, then we use the CustomSubRoomNames to find the specific room we want to spawn the item in within that same business/apartment address. This is optional and can be left empty" />
              </ListItem>
              <ListItem>
                <ListItemIcon><InfoIcon color="primary" fontSize="small" /></ListItemIcon>
                <ListItemText primary="Next we have defined the FurniturePresets list, this will search for any of the FurniturePresets within the defined room, it works the same way as the home/work locations, if no matching furniture is found it will use a random node within the room to spawn, this can also be set to false to use the floor by default, if no sub room is specified, it will search every room in the address for the furniture" />
              </ListItem>
              <ListItem>
                <ListItemIcon><InfoIcon color="primary" fontSize="small" /></ListItemIcon>
                <ListItemText primary="Important Note: All of these values can have as many names and presets as you like, and you can also leave them empty, allowing multiple valid locations, you can mix and match these to your liking, some places may be easier than others to configure, for example if you left everything empty but had CustomRoomNames as 'Basement', it will find every basement (not specific rooms) in the save and consider them a valid location, also the wider the range of search with more valid locations, the longer it will take to spawn the item, with specific configuration, it will more or less be instant, if you were to leave all of this empty, it will deem every location as valid and take a lot longer to choose a place to spawn (leaving furniture empty will not make items spawn on any furniture)" />
              </ListItem>
              <ListItem>
                <ListItemIcon><InfoIcon color="primary" fontSize="small" /></ListItemIcon>
                <ListItemText primary="The second example shows us setting the spawn location to the kitchen within a restaurant. First we find the type of company/business by choosing the room name 'Fast Food Restaurant' and the preset 'FastFoodDiningRoom', there will be multiple of these but it will choose one at random unless we defined a specific floor that one does not match, in this case we included all floor types, in my case here it finds 'Flash Buffalo Buffet Fast Food Restaurant' this will store the company name without the room name 'Fast Food Restaurant' like so 'Flash Buffalo Buffet', when we define the sub room name 'Kitchen' and the preset 'Kitchen', it will find the kitchen within the 'Flash Buffalo Buffet' address like so 'Flash Buffalo Buffet Kitchen', using the company name as the prefix" />
              </ListItem>
              <ListItem>
                <ListItemIcon><InfoIcon color="primary" fontSize="small" /></ListItemIcon>
                <ListItemText primary="You do not have to define both room name and preset, but it makes the search faster, if you want a more lenient search, you can leave one empty depending on how the address works, the game uses many different ways of defining buildings and rooms, such as the diner bathrooms, they are considered their own building, you would simply find it by defining the public bathroom name, but the floor type would be the same as the diner" />
              </ListItem>
              <ListItem>
                <ListItemIcon><InfoIcon color="primary" fontSize="small" /></ListItemIcon>
                <ListItemText primary="Below you can find a link to my DevTools mod, it is very helpful for seeing what room names/presets/floors certain addresses use, and also to see what type of furniture presets certain rooms may use, use the commands /room and /furni to do so. There is also a list of floor names provided for your convenience" />
              </ListItem>
            </List>
            
            <Box sx={{ mt: 2 }}>
              <SyntaxHighlighter language="json" style={vscDarkPlus} wrapLongLines>
                {exampleConfigs.custom.json}
              </SyntaxHighlighter>
            </Box>
          </TabPanel>
        </Box>
        
        <Divider sx={{ my: 3 }} />
        
        <Typography variant="h5" gutterBottom>
          Installation
        </Typography>
        <Typography variant="body1" paragraph>
          To use the configurations created with this tool:
        </Typography>
        <Typography variant="body1" component="ol" sx={{ pl: 2 }}>
          <li>Save the generated JSON configuration</li>
          <li>Place the JSON file in the mod's plugin folder (the JSON file name MUST end in "MIS")</li>
          <li>Reload a save game (you do not need to restart the game as long as the mod is already installed) </li>
        </Typography>
        
        <Box sx={{ mt: 4, p: 2, bgcolor: 'rgba(0, 0, 0, 0.04)', borderRadius: 1 }}>
          <Typography variant="body2" color="textSecondary">
            Note: This tool is designed for use with the MurderItemSpawner mod for Shadows Of Doubt. 
            For more information about the mod itself, please refer to the mod documentation.
          </Typography>
        </Box>
      </Paper>
    </Box>
  );
};

export default Tutorial;