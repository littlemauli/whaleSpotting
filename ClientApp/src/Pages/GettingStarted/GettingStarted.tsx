
import React, { useState } from 'react';
import './GettingStarted.scss';
import { GetLocationData } from './GetLocationData';
import { GetLocationSummary } from './GetLocationData';

import { LocationSummary } from './LocationSummary';
import ReactWeather, { useOpenWeather } from 'react-open-weather';//ignore red-line, it still works

export enum Locations {
    Scotland,
    Canada,
    Australia
}

function GettingStarted() {
    const [selectedLocation, setSelectedLocation] = useState(Locations.Scotland);

    const locationData = GetLocationData(selectedLocation);
    const locationSummary = GetLocationSummary(selectedLocation)


    const { data, isLoading, errorMessage } = useOpenWeather({
        key: '43887186b07bb5deaad592918ae44878',
        lat: locationData.latitude,
        lon: locationData.longitude,
        lang: 'en',
        unit: 'metric'
    });

    return (
        <div>
            <div className="getting_started">

                <div className='top-page-break'></div>

                <h1>Getting Started: Find the Perfect Whalespotting Location</h1>
                <div className="places">
                    <button /*data-testid={"ScotlandButton"}*/ className={selectedLocation === Locations.Scotland ? 'selected-icon' : 'unselected-icon'} onClick={() => setSelectedLocation(Locations.Scotland)}>Scotland</button>
                    <button /*data-testid={"CanadaButton"}*/ className={selectedLocation === Locations.Canada ? 'selected-icon' : 'unselected-icon'} onClick={() => setSelectedLocation(Locations.Canada)}>Canada</button>
                    <button /*data-testid={"AustraliaButton"}*/ className={selectedLocation === Locations.Australia ? 'selected-icon' : 'unselected-icon'} onClick={() => setSelectedLocation(Locations.Australia)}>Australia</button>
                </div>

                <LocationSummary location={locationSummary} />

                <div id="widget">
                    <ReactWeather
                        isLoading={isLoading}
                        errorMessage={errorMessage}
                        data={data}
                        lang="en"
                        locationLabel={locationData.label}
                        unitsLabels={{ temperature: 'C', windSpeed: 'Km/h' }}
                        showForecast
                    />
                </div>
            </div>

            <div className='bottom-page-break'></div>
        </div>
    )
}

export { GettingStarted };


