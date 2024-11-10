import React, { useState } from 'react';
import FeatureCard from '../components/FeatureCard';
import VRDemo from '../components/VRDemo';

const Home: React.FC = () => {
  const [message, setMessage] = useState('');

  return (
    <div className="container mx-auto px-4 py-8">
      {/* Hero Section */}
      <section className="relative h-[600px] flex items-center justify-center overflow-hidden mb-12">
        <div className="absolute inset-0 bg-gradient-to-r from-blue-900/95 to-primary-900/95 z-10"></div>
        <img 
          className="absolute inset-0 w-full h-full object-cover"
          src="https://images.unsplash.com/photo-1584516150909-c43483ee7932?ixlib=rb-1.2.1&auto=format&fit=crop&w=2000&q=80"
          alt="Doctor wearing VR headset in medical training"
        />
        <div className="relative z-20 text-center text-white max-w-4xl mx-auto px-4">
          <h1 className="text-5xl md:text-7xl font-bold mb-6 drop-shadow-lg text-white">Experience Medical Training in VR</h1>
          <p className="text-xl mb-8 text-white text-shadow">Master clinical skills through immersive virtual reality simulations</p>
          <div className="flex flex-col items-center gap-4">
            <div className="flex gap-4">
              <button
                onClick={() => setMessage('Get Your Headset ON')}
                className="bg-accent-purple text-white px-8 py-4 rounded-full font-semibold hover:bg-opacity-90 transition-colors"
              >
                Start Training
              </button>
              <a href="/courses" className="bg-white text-text-primary px-8 py-4 rounded-full font-semibold hover:bg-gray-100 transition-colors">
                Your Progress
              </a>
            </div>
            {message && <p className="text-white text-xl font-bold mt-4">{message}</p>}
          </div>
        </div>
      </section>

      <section className="mb-20">
        <VRDemo />
      </section>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-8 mb-20">
        <FeatureCard
          title="Surgical Instrument Room"
          description="Practice surgical procedures in a risk-free virtual environment with realistic equipment and procedures."
          icon="vr-surgical"
          href="/virtual-or"
        />
        <FeatureCard
          title="Practice Surgery"
          description="Master surgical techniques and procedures in a safe, virtual environment."
          icon="vr-patient"
          href="/virtual-patients"
        />
        <FeatureCard
          title="Anatomy Explorer"
          description="Explore detailed anatomical structures in an interactive 3D space."
          icon="vr-anatomy"
          href="/anatomy"
        />
      </div>

      <section className="bg-gradient-to-r from-accent-purple to-accent-blue text-white p-12 rounded-2xl mb-20">
        <h2 className="text-3xl font-bold mb-12 text-center">Training Impact</h2>
        <div className="grid grid-cols-1 md:grid-cols-4 gap-8">
          <div className="text-center">
            <div className="text-5xl font-bold mb-2">98%</div>
            <div className="text-gray-200">Training Effectiveness</div>
          </div>
          <div className="text-center">
            <div className="text-5xl font-bold mb-2">50K+</div>
            <div className="text-gray-200">Medical Students</div>
          </div>
          <div className="text-center">
            <div className="text-5xl font-bold mb-2">1000+</div>
            <div className="text-gray-200">VR Scenarios</div>
          </div>
          <div className="text-center">
            <div className="text-5xl font-bold mb-2">200+</div>
            <div className="text-gray-200">Medical Schools</div>
          </div>
        </div>
      </section>
    </div>
  );
};

export default Home;