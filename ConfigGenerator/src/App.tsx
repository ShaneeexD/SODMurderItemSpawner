// App component
import { ThemeProvider, createTheme, CssBaseline } from '@mui/material';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import ConfigForm from './components/ConfigForm';
import Tutorial from './components/Tutorial';
import Resources from './components/Resources';
import Navigation from './components/Navigation';
import './App.css';

const theme = createTheme({
  palette: {
    mode: 'dark',
    primary: {
      main: '#9c27b0',
    },
    secondary: {
      main: '#f50057',
    },
  },
});

function App() {
  return (
    <BrowserRouter>
      <ThemeProvider theme={theme}>
        <CssBaseline />
        <div className="app-wrapper">
          <Navigation />
          <div className="content-container">
            <Routes>
              <Route path="/" element={
                <div className="form-wrapper">
                  <ConfigForm />
                </div>
              } />
              <Route path="/tutorial" element={<Tutorial />} />
              <Route path="/resources" element={<Resources />} />
            </Routes>
          </div>
        </div>
      </ThemeProvider>
    </BrowserRouter>
  );
}

export default App
