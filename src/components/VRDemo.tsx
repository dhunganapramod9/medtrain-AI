import React, { useState } from 'react';

interface TabContent {
  title: string;
  description: string;
}

const tabs: Record<string, TabContent> = {
  or: {
    title: 'Surgical Instrument Room',
    description: 'Experience a fully immersive surgical instrument environment with realistic equipment and procedures.'
  },
  patient: {
    title: 'Practice Surgery',
    description: 'Master surgical techniques and procedures in a safe, virtual environment.'
  },
  anatomy: {
    title: 'Anatomy Explorer',
    description: 'Explore detailed anatomical structures in an interactive 3D space.'
  }
};

const VRDemo = () => {
  const [activeTab, setActiveTab] = useState('or');
  const [message, setMessage] = useState('');

  return (
    <div className="bg-white rounded-2xl overflow-hidden">
      <div className="grid grid-cols-1 lg:grid-cols-2 min-h-[500px] relative">
        {/* VR Preview Area */}
        <div className="relative bg-[#0F172A] flex items-center justify-center h-full">
          <div className="text-white text-center">
            <div className="mb-4">
              <svg className="w-16 h-16 mx-auto" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 10l4.553-2.276A1 1 0 0121 8.618v6.764a1 1 0 01-1.447.894L15 14M5 18h8a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v8a2 2 0 002 2z" />
              </svg>
            </div>
            <p className="text-lg mb-4">Interactive VR Preview</p>
            <button 
              onClick={() => setMessage('Get Your Headset ON')}
              className="px-8 py-3 bg-[#00A7E1] text-white rounded-full hover:bg-opacity-90 transition-colors"
            >
              Start Training
            </button>
          </div>
        </div>
        <div className="absolute bottom-4 left-0 right-0 text-center">
          {message && <p className="text-white text-xl font-bold">{message}</p>}
        </div>

        {/* White divider line */}
        <div className="absolute top-0 bottom-0 left-1/2 w-[1px] bg-white hidden lg:block"></div>

        {/* Content Area */}
        <div className="p-6 flex flex-col justify-center bg-[#0F172A]">
          <h1 className="text-3xl font-bold mb-6 text-white">Experience Medical Training in VR</h1>
          
          <div className="flex gap-3 mb-6">
            <button
              onClick={() => setActiveTab('or')}
              className={`px-4 py-2 rounded-full text-sm font-medium transition-colors whitespace-nowrap ${
                activeTab === 'or'
                  ? 'bg-[#00A7E1] text-white'
                  : 'bg-gray-100 text-gray-600 hover:bg-gray-200'
              }`}
            >
              Surgical Instrument Room
            </button>
            <button
              onClick={() => setActiveTab('patient')}
              className={`px-4 py-2 rounded-full text-sm font-medium transition-colors ${
                activeTab === 'patient'
                  ? 'bg-[#00A7E1] text-white'
                  : 'bg-gray-100 text-gray-600 hover:bg-gray-200'
              }`}
            >
              Practice Surgery
            </button>
            <button
              onClick={() => setActiveTab('anatomy')}
              className={`px-4 py-2 rounded-full text-sm font-medium transition-colors ${
                activeTab === 'anatomy'
                  ? 'bg-[#00A7E1] text-white'
                  : 'bg-gray-100 text-gray-600 hover:bg-gray-200'
              }`}
            >
              Anatomy Explorer
            </button>
          </div>

          <div className="mb-6">
            <h2 className="text-2xl font-bold mb-2 text-white">{tabs[activeTab].title}</h2>
            <p className="text-gray-300 text-base">{tabs[activeTab].description}</p>
          </div>

          <div className="space-y-4">
            <div className="flex items-center gap-3">
              <div className="w-5 h-5 rounded-full bg-[#00A7E1] flex items-center justify-center text-white">
                ✓
              </div>
              <span className="text-base text-white">Haptic feedback for realistic tool handling</span>
            </div>
            <div className="flex items-center gap-3">
              <div className="w-5 h-5 rounded-full bg-[#00A7E1] flex items-center justify-center text-white">
                ✓
              </div>
              <span className="text-base text-white">Real-time performance feedback</span>
            </div>
            <div className="flex items-center gap-3">
              <div className="w-5 h-5 rounded-full bg-[#00A7E1] flex items-center justify-center text-white">
                ✓
              </div>
              <span className="text-base text-white">Multi-user training sessions</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default VRDemo;